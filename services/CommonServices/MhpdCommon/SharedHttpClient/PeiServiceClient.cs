using MhpdCommon.Constants;
using MhpdCommon.Constants.HttpClient;
using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.Models.MHPDModels;
using Microsoft.Extensions.Logging;

namespace MhpdCommon.SharedHttpClient;

public class PeiServiceClient(IHttpClientFactory httpClientFactory, ILogger<PeiServiceClient> logger) 
    : BaseHttpClientExecutor(httpClientFactory, logger), IPeiServiceClient
{
    public async Task<CdaPeisServiceResponseModel> GetPeiDataAsync(PeiRequestModel request)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Iss);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.PeisId);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.UserSessionId);

        logger.LogWarning("Sending HTTP request for Pei data to {Service} with Peis Id: [{PeisId}], user session Id: [{UserSessionId}] and rpt: [{Rpt}]",
            HttpClientNames.PeiIntegrationService, request.PeisId, request.UserSessionId, request.Rpt);

        var peiResponse = await ExecuteAsync<CdaPeisServiceResponseModel>(HttpClientNames.PeiIntegrationService,
            _ => new HttpRequestMessage(HttpMethod.Get, HttpEndpoints.Internal.IntegrationPeis),
            HttpClientOperationName.PeiServicePeisGet,
        message =>
        {
            message.Headers.Add(HeaderConstants.Iss, request.CorrelationId);
            message.Headers.Add(HeaderConstants.PeisId, request.CorrelationId);
            message.Headers.Add(HeaderConstants.UserSessionId, request.CorrelationId);
            message.Headers.Add(HeaderConstants.CorrelationId, request.CorrelationId);
            message.Headers.Add(HeaderConstants.Rpt, request.CorrelationId);
        });

        return peiResponse!;
    }
}
