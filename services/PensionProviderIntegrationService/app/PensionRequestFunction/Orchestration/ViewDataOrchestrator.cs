using MhpdCommon.Constants;
using MhpdCommon.Utils;
using Microsoft.Extensions.Logging;
using PensionRequestFunction.Constants;
using PensionRequestFunction.HttpClient;
using PensionRequestFunction.HttpClient.Interfaces;
using PensionRequestFunction.Models.CdaPeisServiceClient;
using PensionRequestFunction.Models.MapsRqpServiceClient;
using PensionRequestFunction.Models.TokenIntegrationServiceClient;
using Polly;
using System.Net;
using System.Text.Json;

namespace PensionRequestFunction.Orchestration;

public class ViewDataOrchestrator(ILogger<ViewDataOrchestrator> logger,
    IIdValidator validator,
    IHolderNameClient holderNameClient,
    IPdpViewDataClient viewDataClient,
    ITokenIntegrationServiceClient tokenClient,
    IMapsCdaServiceClient rqpClient,
    ITokenUtility tokenUtility) : IViewDataOrchestrator
{
    private readonly IPdpViewDataClient _pdpViewDataClient = viewDataClient;
    private readonly IMapsCdaServiceClient _iMapsRqpService = rqpClient;
    private readonly ILogger<ViewDataOrchestrator> _logger = logger;
    private readonly ITokenIntegrationServiceClient _iTokenIntegrationService = tokenClient;
    private readonly IIdValidator _idValidator = validator;
    private readonly IHolderNameClient _holderNameClient = holderNameClient;
    private readonly ITokenUtility _tokenUtility = tokenUtility;

    public async Task<string?> GetPensionViewDataAsync(string pei, string iss, string userSessionId, string correlationId)
    {
        if (!_idValidator.TryExtractPei(pei, out string holderNameGuid, out _))
        {
            throw new FormatException(StatusConstants.InvalidPei);
        }

        var viewDataUrl = await GetViewDataUrlAsync(holderNameGuid);

        if (viewDataUrl == null)
        {
            throw new InvalidOperationException(string.Format(StatusConstants.NoViewDataUrl, pei));
        }
        
        var viewDataToken = await GetViewDataAsync(correlationId, viewDataUrl, pei, iss, userSessionId, null);

        return _tokenUtility.RetrieveClaim(viewDataToken, "view_data");
    }

    private async Task<string?> GetViewDataUrlAsync(string holderNameGuid)
    {
        var viewData = await _holderNameClient.GetViewDataUrlAsync(holderNameGuid);

        return viewData?.ViewDataUrl;
    }

    private async Task<string> GetViewDataAsync(string correlationId, string viewDataUrl, string pei, string iss, string userSessionId, string? rpt)
    {
        _idValidator.TryExtractPei(pei, out _, out string externalAssetId);
        PdpServiceResponseModel? responseModel = null;

        var retryPolicy = Policy
            .HandleResult<PdpServiceResponseModel>(r => r.ResponseMessage?.ResponseStatusCode == HttpStatusCode.Unauthorized.ToString())
            .WaitAndRetryAsync(1, retryAttempt => TimeSpan.Zero, async (result, timeSpan, retryCount, context) =>
            {
                _logger.LogInformation(StatusConstants.FetchingRpt, correlationId);
                rpt = await DoAuthenticationDance(result.Result, iss, userSessionId);
            });

        await retryPolicy.ExecuteAsync(async () =>
        {
            responseModel = await _pdpViewDataClient.GetPdpViewDataAsync(externalAssetId, viewDataUrl, rpt);
            return responseModel;
        });

        var responseDocument = JsonDocument.Parse(responseModel!.ViewDataToken!);
        var viewDataTokenExists = responseDocument.RootElement.TryGetProperty("view_data_token", out JsonElement viewDataClaimValue);

        if (!viewDataTokenExists || viewDataClaimValue.ValueKind == JsonValueKind.Undefined)
        {
            return string.Empty;
        }

        var viewDataString = viewDataClaimValue.ToString();

        return viewDataString;
    }

    private async Task<string> DoAuthenticationDance(PdpServiceResponseModel viewDataResponse, string iss, string userSessionId)
    {
        var rqpResponse = await _iMapsRqpService.PostRqpAsync(new MapsRqpServiceRequestModel { Iss = iss, UserSessionId = userSessionId });

        var rptResponse = await RetrieveRptAsync(viewDataResponse, rqpResponse.Rqp);
        return rptResponse.Rpt!;
    }

    private async Task<TokenIntegrationResponseModel> RetrieveRptAsync(PdpServiceResponseModel pdpServiceResponseModel, string? rqp)
    {
        var ticketValue = ExtractWWWAuthenticateHeaderValue(pdpServiceResponseModel.ResponseMessage.WWWAuthenticateResponseHeader!, HeaderConstants.AuthenticateTicket);
        var asUriValue = ExtractWWWAuthenticateHeaderValue(pdpServiceResponseModel.ResponseMessage.WWWAuthenticateResponseHeader!, HeaderConstants.AuthenticateUri);

        var tokenIntegrationServiceRequestModel = new TokenIntegrationServiceRequestModel
        {
            Ticket = ticketValue,
            Rqp = rqp,
            As_Uri = asUriValue
        };

        var tokenIntegrationResponseModel = await _iTokenIntegrationService.PostRptAsync(tokenIntegrationServiceRequestModel);

        return tokenIntegrationResponseModel;
    }

    private static string ExtractWWWAuthenticateHeaderValue(string wwwAuthenticateHeader, string tokenToExtract)
    {
        var token = wwwAuthenticateHeader.Split(tokenToExtract)[1];
        return token.Split(",")[0].Replace("\"", "");
    }
}
