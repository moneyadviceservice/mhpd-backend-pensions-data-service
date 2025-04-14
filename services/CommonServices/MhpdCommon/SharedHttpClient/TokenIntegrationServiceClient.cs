using MhpdCommon.Constants;
using MhpdCommon.Constants.HttpClient;
using MhpdCommon.Extensions;
using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.Models.MHPDModels;
using MhpdCommon.Utils;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace MhpdCommon.SharedHttpClient;

public class TokenIntegrationServiceClient(ILogger<TokenIntegrationServiceClient> logger, IHttpClientFactory httpClientFactory)
    : BaseHttpClientExecutor(httpClientFactory, logger), ITokenIntegrationServiceClient
{
    public async Task<CdaTokenResponseModel> PostAccessTokenAsync(TokenClientRequestModel request, string? correlationId = null)
    {
        logger.LogRequest(request);

        var response = await ExecuteAsync<CdaTokenResponseModel>(HttpClientNames.TokenIntegrationService,
            _ =>
            {
                var payload = JsonSerializer.Serialize(request);
                return new HttpRequestMessage(HttpMethod.Post, HttpEndpoints.Internal.Rpts)
                {
                    Content = new StringContent(payload, Encoding.UTF8, "application/json")
                };
            },
            HttpClientOperationName.TokenServiceAccessToken,
        message => message.Headers.Add(HeaderConstants.CorrelationId, GetCorrelationId(correlationId)));

        return response;
    }

    public async Task<PeiRetrievalDetailsResponseModel> PostIdTokenAsync(PensionsDataRequestModel request, string? correlationId = null)
    {
        logger.LogRequest(request);

        var response = await ExecuteAsync<PeiRetrievalDetailsResponseModel>(HttpClientNames.TokenIntegrationService,
            _ => new HttpRequestMessage(HttpMethod.Post, UrlHelper.ConstructEndPoint(request, HttpEndpoints.Internal.PeiRetrievalDetails)),
            HttpClientOperationName.TokenServiceIdToken,
        message => message.Headers.Add(HeaderConstants.CorrelationId, GetCorrelationId(correlationId)));

        return response;
    }

    private static string GetCorrelationId(string? correlationId)
    {
        if (string.IsNullOrWhiteSpace(correlationId))
        {
            return Guid.NewGuid().ToString();
        }

        return correlationId;
    }
}
