using MhpdCommon.Constants;
using MhpdCommon.Constants.HttpClient;
using MhpdCommon.Extensions;
using PensionsRetrievalFunction.Models;
using System.Net.Http.Json;

namespace PensionsRetrievalFunction.HttpClients;

public class PeiServiceClient(IHttpClientFactory httpClientFactory) : IPeiServiceClient
{
    public async Task<PeiDataResponse> GetPeiDataAsync(string? rpt, string? iss, string? peisId, string? userSessionId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(iss);
        ArgumentException.ThrowIfNullOrWhiteSpace(peisId);
        ArgumentException.ThrowIfNullOrWhiteSpace(userSessionId);

        var client = httpClientFactory.CreateClient(HttpClientNames.PeiIntegrationService);
        client.DefaultRequestHeaders.Add(HeaderConstants.Iss, iss);
        client.DefaultRequestHeaders.Add(HeaderConstants.PeisId, peisId);
        client.DefaultRequestHeaders.Add(HeaderConstants.UserSessionId, userSessionId);
        if (string.IsNullOrEmpty(rpt))
            client.DefaultRequestHeaders.Add(HeaderConstants.Rpt, rpt);

        var response = await client.GetAsync(HttpEndpoints.Internal.IntegrationPeis);
        if (!response.IsSuccessStatusCode) return new PeiDataResponse(null, []);

        var peiData = await response.Content.ReadFromJsonAsync<List<PeiData>>();

        return new PeiDataResponse(response.GetResponseHeader(HeaderConstants.Rpt), peiData ?? []);
    }
}
