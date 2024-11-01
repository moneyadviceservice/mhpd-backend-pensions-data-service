using MhpdCommon.Constants;
using MhpdCommon.Constants.HttpClient;
using MhpdCommon.Extensions;
using Microsoft.Extensions.Logging;
using PensionsRetrievalFunction.Models;
using System.Net.Http.Json;

namespace PensionsRetrievalFunction.HttpClients;

public class PeiServiceClient(IHttpClientFactory httpClientFactory, ILogger<PeiServiceClient> logger) : IPeiServiceClient
{
    public async Task<PeiDataResponse> GetPeiDataAsync(PeiRequest request)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Iss);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.PeisId);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.UserSessionId);

        logger.LogWarning("Sending HTTP request for Pei data to {service} with Peis Id: [{peisId}], user session Id: [{userSessionId}] and rpt: [{rpt}]", 
            HttpClientNames.PeiIntegrationService, request.PeisId, request.UserSessionId, request.Rpt);

        var client = httpClientFactory.CreateClient(HttpClientNames.PeiIntegrationService);
        client.DefaultRequestHeaders.Add(HeaderConstants.Iss, request.Iss);
        client.DefaultRequestHeaders.Add(HeaderConstants.PeisId, request.PeisId);
        client.DefaultRequestHeaders.Add(HeaderConstants.UserSessionId, request.UserSessionId);
        client.DefaultRequestHeaders.Add(HeaderConstants.CorrelationId, request.CorrelationId);

        if (!string.IsNullOrEmpty(request.Rpt))
        {
            client.DefaultRequestHeaders.Add(HeaderConstants.Rpt, request.Rpt);
        }

        var response = await client.GetAsync(HttpEndpoints.Internal.IntegrationPeis);
        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("Response {code} from {service} did not indicate success", response.StatusCode, HttpClientNames.PeiIntegrationService);
            return new PeiDataResponse(null, []);
        }

        var peiData = await response.Content.ReadFromJsonAsync<List<PeiData>>();

        var peiResponse = new PeiDataResponse(response.GetResponseHeader(HeaderConstants.Rpt), peiData ?? []);

        logger.LogResponse(peiResponse);

        return peiResponse;
    }
}
