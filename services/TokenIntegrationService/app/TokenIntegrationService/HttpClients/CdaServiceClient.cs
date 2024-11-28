using MhpdCommon.Constants;
using MhpdCommon.Constants.HttpClient;
using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.SharedHttpClient;
using MhpdCommon.Utils;
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

                var endpoint = request switch
                {
                    TokenIntegrationRequestModel tokenRequest => UrlHelper.ConstructEndPoint(
                        tokenRequest, HttpEndpoints.External.CdaTokenServiceEndpoint),
                    CdaTokenRequestModel cdaTokenRequest => UrlHelper.ConstructEndPoint(
                        cdaTokenRequest, HttpEndpoints.External.CdaTokenServiceEndpoint),
                    _ => throw new InvalidOperationException("Unsupported request type.")
                };

                return await httpClient.PostAsync(endpoint, null);
            },
            HttpClientOperationName.CdaServiceTokenPOST);
    }
}