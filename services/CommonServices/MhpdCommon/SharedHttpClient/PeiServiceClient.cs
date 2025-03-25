using MhpdCommon.Constants;
using MhpdCommon.Constants.HttpClient;
using MhpdCommon.Extensions;
using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.Models.MHPDModels;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace MhpdCommon.SharedHttpClient;

public class PeiServiceClient(IHttpClientFactory httpClientFactory, ILogger<PeiServiceClient> logger) : IPeiServiceClient
{
    public async Task<CdaPeisServiceResponseModel> GetPeiDataAsync(PeiRequestModel request)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Iss);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.PeisId);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.UserSessionId);

        logger.LogWarning("Sending HTTP request for Pei data to {Service} with Peis Id: [{PeisId}], user session Id: [{UserSessionId}] and rpt: [{Rpt}]",
            HttpClientNames.PeiIntegrationService, request.PeisId, request.UserSessionId, request.Rpt);

        var client = httpClientFactory.CreateClient(HttpClientNames.PeiIntegrationService);
        client.DefaultRequestHeaders.Add(HeaderConstants.Iss, request.Iss);
        client.DefaultRequestHeaders.Add(HeaderConstants.PeisId, request.PeisId);
        client.DefaultRequestHeaders.Add(HeaderConstants.UserSessionId, request.UserSessionId);
        client.DefaultRequestHeaders.Add(HeaderConstants.CorrelationId, request.CorrelationId);
        client.DefaultRequestHeaders.Add(HeaderConstants.Rpt, request.Rpt);

        var response = await client.GetAsync(HttpEndpoints.Internal.IntegrationPeis);

        response.EnsureSuccessStatusCode();

        var peiResponse = await response.Content.ReadFromJsonAsync<CdaPeisServiceResponseModel>();

        logger.LogResponse(peiResponse);

        return peiResponse!;
    }
}
