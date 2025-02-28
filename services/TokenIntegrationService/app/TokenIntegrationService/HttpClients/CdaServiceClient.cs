using MhpdCommon.Constants;
using MhpdCommon.Constants.HttpClient;
using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.SharedHttpClient;
using MhpdCommon.Utils;
using System.Text;
using System.Text.Json;
using TokenIntegrationService.Models;

namespace TokenIntegrationService.HttpClients;

public class CdaServiceClient(IHttpClientFactory httpClientFactory, ILogger<CdaServiceClient> logger) : 
    BaseHttpClientExecutor(httpClientFactory, logger), ICdaServiceClient
{
    public async Task<CdaTokenResponseModel> PostAsync<TRequest>(TRequest request)
    {
        return await ExecuteAsync<CdaTokenResponseModel>(
            HttpClientNames.CdaService,
            async httpClient =>
            {
                var requestId = Guid.NewGuid().ToString();
                logger.LogWarning("Sending request to token endpoint with request Id: {RequestId}", requestId);

                httpClient.DefaultRequestHeaders.Add(HeaderConstants.RequestId, requestId);

                var payload = request switch
                {
                    TokenIntegrationRequestModel tokenRequest => UrlHelper.ConstructFormEncodedPayload(tokenRequest),
                    CdaTokenRequestModel cdaTokenRequest => UrlHelper.ConstructFormEncodedPayload(cdaTokenRequest),
                    _ => throw new InvalidOperationException("Unsupported request type.")
                };

                return await httpClient.PostAsync(HttpEndpoints.External.CdaTokenServiceEndpoint, payload);
            },
            HttpClientOperationName.CdaServiceTokenPOST);
    }
}