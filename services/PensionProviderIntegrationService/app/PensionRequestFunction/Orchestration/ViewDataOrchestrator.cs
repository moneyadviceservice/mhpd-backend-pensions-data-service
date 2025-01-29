using MhpdCommon.Constants;
using MhpdCommon.Utils;
using Microsoft.Extensions.Logging;
using PensionRequestFunction.Constants;
using PensionRequestFunction.Models.CdaPeisServiceClient;
using PensionRequestFunction.Models.MapsRqpServiceClient;
using PensionRequestFunction.Models.TokenIntegrationServiceClient;
using Polly;
using System.Net;
using System.Text.Json;
using PensionRequestFunction.HttpClient;
using PensionRequestFunction.HttpClient.Interfaces;

namespace PensionRequestFunction.Orchestration;

public class ViewDataOrchestrator(ILogger<ViewDataOrchestrator> logger,
    IIdValidator validator,
    ITokenUtility tokenUtility,
    IJwtUtility jwtUtility,
    ViewDataOrchestratorClients orchestratorClients) : IViewDataOrchestrator
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
            .HandleResult<PdpServiceResponseModel>(r => r.ResponseMessage?.ResponseStatusCode == HttpStatusCode.Unauthorized.ToString())
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
        var rqpResponse = await _rqpClient.PostRqpAsync(new MapsRqpServiceRequestModel { Iss = iss, UserSessionId = userSessionId, CorrelationId = correlationId });
        
        var rptResponse = await RetrieveRptAsync(viewDataResponse, rqpResponse.Rqp, correlationId);
        return rptResponse.Rpt!;
    }

    private async Task<TokenIntegrationResponseModel> RetrieveRptAsync(PdpServiceResponseModel pdpServiceResponseModel, string? rqp, string correlationId)
    {
        var ticketValue = ExtractWwwAuthenticateHeaderValue(pdpServiceResponseModel.ResponseMessage.WWWAuthenticateResponseHeader!, HeaderConstants.AuthenticateTicket);
        var asUriValue = ExtractWwwAuthenticateHeaderValue(pdpServiceResponseModel.ResponseMessage.WWWAuthenticateResponseHeader!, HeaderConstants.AuthenticateUri);

        var tokenIntegrationServiceRequestModel = new TokenIntegrationServiceRequestModel
        {
            Ticket = ticketValue,
            Rqp = rqp,
            As_Uri = asUriValue,
            CorrelationId = correlationId
        };

        var tokenIntegrationResponseModel = await _tokenClient.PostRptAsync(tokenIntegrationServiceRequestModel);
        
        return tokenIntegrationResponseModel;
    }

    private static string ExtractWwwAuthenticateHeaderValue(string wwwAuthenticateHeader, string tokenToExtract)
    {
        var token = wwwAuthenticateHeader.Split(tokenToExtract)[1];
        return token.Split(",")[0].Replace("\"", "");
    }
}
