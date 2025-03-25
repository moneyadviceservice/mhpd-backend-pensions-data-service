using MhpdCommon.Constants;
using MhpdCommon.Utils;
using Microsoft.Extensions.Logging;
using PensionRequestFunction.Constants;
using PensionRequestFunction.Models.CdaPeisServiceClient;
using Polly;
using System.Net;
using System.Text.Json;
using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.Models.RequestHeaderModel;
using MhpdCommon.Repository;
using MhpdCommon.SharedHttpClient;
using PensionRequestFunction.HttpClient;

namespace PensionRequestFunction.Orchestration;

public class ViewDataOrchestrator(ILogger<ViewDataOrchestrator> logger,
    IIdValidator validator,
    ITokenUtility tokenUtility,
    IJwtUtility jwtUtility,
    ViewDataOrchestratorClients orchestratorClients,
    UserSessionDataRepository userSessionDataRepository) : IViewDataOrchestrator
{
    private readonly IHolderNameClient _holderNameClient = orchestratorClients.HolderNameClient;
    private readonly IPdpViewDataClient _viewDataClient = orchestratorClients.ViewDataClient;
    private readonly ITokenIntegrationServiceClient _tokenClient = orchestratorClients.TokenClient;
    private readonly IMapsCdaServiceClient _rqpClient = orchestratorClients.RqpClient;
    
    public async Task<string?> GetPensionViewDataAsync(string pei, string iss, string userSessionId, string correlationId)
    {
        if (!validator.TryExtractPei(pei, out var holderNameGuid, out _))
        {
            throw new FormatException(StatusConstants.InvalidPei);
        }

        var viewDataUrl = await GetViewDataUrlAsync(holderNameGuid, correlationId) ?? 
            throw new InvalidOperationException(string.Format(StatusConstants.NoViewDataUrl, pei));

        logger.LogWarning("Accessing {viewDataUrl} for view data...", viewDataUrl);

        var viewDataToken = await GetViewDataAsync(correlationId, viewDataUrl, pei, iss, userSessionId, null);

        return tokenUtility.RetrieveClaim(viewDataToken, "view_data");
    }

    private async Task<string?> GetViewDataUrlAsync(string holderNameGuid, string correlationId)
    {
        var viewData = await _holderNameClient.GetViewDataUrlAsync(holderNameGuid, correlationId);

        return viewData?.ViewDataUrl;
    }

    private async Task<string> GetViewDataAsync(string correlationId, string viewDataUrl, string pei, string iss, string userSessionId, string? rpt)
    {
        validator.TryExtractPei(pei, out _, out var externalAssetId);
        PdpServiceResponseModel? responseModel = null;

        var retryPolicy = Policy
            .HandleResult<PdpServiceResponseModel>(r => r.ResponseMessage.ResponseStatusCode == HttpStatusCode.Unauthorized)
            .WaitAndRetryAsync(1, retryAttempt => TimeSpan.Zero, async (result, timeSpan, retryCount, context) =>
            {
                logger.LogWarning(StatusConstants.FetchingRpt, correlationId);
                rpt = await DoAuthenticationDance(result.Result, iss, userSessionId, correlationId);
            });

        await retryPolicy.ExecuteAsync(async () =>
        {
            responseModel = await _viewDataClient.GetPdpViewDataAsync(externalAssetId, viewDataUrl, rpt, correlationId);
            return responseModel;
        });

        var responseDocument = JsonDocument.Parse(responseModel!.ViewDataToken!);

        if(!responseDocument.RootElement.TryGetProperty("view_data_token", out JsonElement viewDataClaimValue) ||
            viewDataClaimValue.ValueKind == JsonValueKind.Undefined)
        {
            return string.Empty;
        }
        
        try
        {
            // Validate the Jwt token signature
            await jwtUtility.ValidateJwtTokenWithKidAsync(viewDataClaimValue.ToString(), logger);
        } 
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while validating the view_data_token signature");
            throw new InvalidOperationException("An error occurred while validating the view_data_token signature.", ex);
        }

        return viewDataClaimValue.ToString();
    }

    private async Task<string> DoAuthenticationDance(PdpServiceResponseModel viewDataResponse, string iss, string userSessionId, string correlationId)
    {
        var rqpResponse = await _rqpClient.PostRqp(new RequestHeaderModel { Iss = iss, UserSessionId = userSessionId, CorrelationId = correlationId });
        if (string.IsNullOrEmpty(rqpResponse.Rqp))
        {
            logger.LogWarning(StatusConstants.FetchingRqp, correlationId);
            throw new FormatException(StatusConstants.FetchingRqp); 
        }
        
        var rptResponse = await RetrieveRptAsync(viewDataResponse, rqpResponse.Rqp, userSessionId, correlationId);
        return rptResponse.AccessToken!;
    }

    private async Task<CdaTokenResponseModel> RetrieveRptAsync(PdpServiceResponseModel pdpServiceResponseModel, string rqp, string userSessionId, string correlationId)
    {
        var ticketValue = ExtractWwwAuthenticateHeaderValue(pdpServiceResponseModel.ResponseMessage.WwwAuthenticateResponseHeader!, HeaderConstants.AuthenticateTicket);
        var asUriValue = ExtractWwwAuthenticateHeaderValue(pdpServiceResponseModel.ResponseMessage.WwwAuthenticateResponseHeader!, HeaderConstants.AuthenticateUri);
        
        Console.WriteLine("Retrieving userSessionData {0}", userSessionId);
        
        // Retrieve PCT and pass it downstream
        var userSessionData = await userSessionDataRepository.GetByIdAsync(userSessionId, userSessionId);
        if (userSessionData == null)
        {
            logger.LogWarning(StatusConstants.UserSessionDataNotFound, userSessionId, correlationId);
            throw new FormatException(StatusConstants.UserSessionDataNotFound);
        }

        if (string.IsNullOrEmpty(userSessionData.Pct))
        {
            logger.LogWarning(StatusConstants.NoPctFound, userSessionId);
            throw new FormatException(StatusConstants.NoPctFound);
        }
        
        var request = new TokenClientRequestModel
        {
            Ticket = ticketValue,
            Rqp = rqp,
            AsUri = asUriValue,
            CorrelationId = correlationId,
            Pct = userSessionData.Pct
        };

        Console.WriteLine("Retrieving RPT {0}", request);
        return await _tokenClient.PostRptAsync(request);
    }

    private static string ExtractWwwAuthenticateHeaderValue(string wwwAuthenticateHeader, string tokenToExtract)
    {
        var token = wwwAuthenticateHeader.Split(tokenToExtract)[1];
        return token.Split(",")[0].Replace("\"", "");
    }
}
