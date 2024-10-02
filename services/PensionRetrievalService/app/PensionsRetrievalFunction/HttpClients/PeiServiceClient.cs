using MhpdCommon.Constants;
using MhpdCommon.Extensions;
using Microsoft.Extensions.Options;
using PensionsRetrievalFunction.Models;
using System.Net.Http.Json;

namespace PensionsRetrievalFunction.HttpClients;

public class PeiServiceClient(HttpClient httpClient, IOptions<MhpdApiConfiguration> options) : IPeiServiceClient
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly MhpdApiConfiguration _configuration = options.Value;

    public async Task<PeiDataResponse> GetPeiDataAsync(string? rpt, string? iss, string? peisId, string? userSessionId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(iss);
        ArgumentException.ThrowIfNullOrWhiteSpace(peisId);
        ArgumentException.ThrowIfNullOrWhiteSpace(userSessionId);

        var request = new HttpRequestMessage(HttpMethod.Get, Combine(_configuration.PeiIntegrationApi, Constants.PeiDataEndpoint));
        request.Headers.Add(HeaderConstants.Iss, iss);
        request.Headers.Add(HeaderConstants.PeisId, peisId);
        request.Headers.Add(HeaderConstants.UserSessionId, userSessionId);
        if (string.IsNullOrEmpty(rpt)) 
            request.Headers.Add(HeaderConstants.Rpt, rpt);

        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode) return new PeiDataResponse(null, []);

        var peiData = await response.Content.ReadFromJsonAsync<List<PeiData>>();

        return new PeiDataResponse(response.GetResponseHeader(HeaderConstants.Rpt), peiData ?? []);
    }

    private static string Combine(string? baseUrl, string? relativeUrl)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
            throw new ArgumentNullException(nameof(baseUrl));

        if (string.IsNullOrWhiteSpace(relativeUrl))
            return baseUrl;

        baseUrl = baseUrl.TrimEnd('/');
        relativeUrl = relativeUrl.TrimStart('/');

        return $"{baseUrl}/{relativeUrl}";
    }
}
