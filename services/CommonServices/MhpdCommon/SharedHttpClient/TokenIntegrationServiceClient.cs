using MhpdCommon.Constants;
using MhpdCommon.Constants.HttpClient;
using MhpdCommon.Extensions;
using MhpdCommon.Models.MessageBodyModels;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace MhpdCommon.SharedHttpClient;

public class TokenIntegrationServiceClient(ILogger<TokenIntegrationServiceClient> logger, IHttpClientFactory httpClientFactory)
    : ITokenIntegrationServiceClient
{
    public async Task<CdaTokenResponseModel> PostRptAsync(TokenClientRequestModel request)
    {
        logger.LogRequest(request);

        var client = httpClientFactory.CreateClient(HttpClientNames.TokenIntegrationService);

        client.DefaultRequestHeaders.Add(HeaderConstants.CorrelationId, request.CorrelationId);

        request.AsUri = request.AsUri;
        var payload = JsonSerializer.Serialize(request);

        var content = new StringContent(payload, Encoding.UTF8, "application/json");
        var response = await client.PostAsync(HttpEndpoints.Internal.Rpts, content);
        response.EnsureSuccessStatusCode();

        var result = response.Content.ReadFromJsonAsync<CdaTokenResponseModel>().Result;
        logger.LogResponse(result);
        return result!;
    }
}
