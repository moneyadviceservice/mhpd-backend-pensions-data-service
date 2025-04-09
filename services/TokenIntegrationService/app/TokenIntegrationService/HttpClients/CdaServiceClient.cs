using MhpdCommon.Constants;
using MhpdCommon.Constants.HttpClient;
using MhpdCommon.CustomExceptions;
using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.SharedHttpClient;
using MhpdCommon.Utils;
using System.Net;
using System.Text.Json;
using TokenIntegrationService.Models;

namespace TokenIntegrationService.HttpClients;

public class CdaServiceClient(IHttpClientFactory httpClientFactory, ILogger<CdaServiceClient> logger) 
    : BaseHttpClientExecutor(httpClientFactory, logger), ICdaServiceClient
{
    public async Task<CdaTokenResponseModel> PostAsync<TRequest>(TRequest request)
    {
        var requestId = Guid.NewGuid().ToString();
        logger.LogWarning("Sending request to token endpoint with request Id: {RequestId}", requestId);

        CdaTokenResponseModel? result = null;

        try
        {
            var response = await ExecuteAsync(
            HttpClientNames.CdaService,
            HttpClientOperationName.CdaServiceTokenPost,
            httpClient =>
            {
                var payload = request switch
                {
                    TokenClientRequestModel tokenRequest => UrlHelper.ConstructFormEncodedPayload(tokenRequest),
                    CdaTokenRequestModel cdaTokenRequest => UrlHelper.ConstructFormEncodedPayload(cdaTokenRequest),
                    _ => throw new InvalidOperationException("Unsupported request type.")
                };

                return new HttpRequestMessage(HttpMethod.Post, HttpEndpoints.External.CdaTokenServiceEndpoint)
                {
                    Content = payload,
                };
            },
            message => message.Headers.Add(HeaderConstants.RequestId, requestId));

            if (response.IsSuccessStatusCode)
            {
                result = await response.Content.ReadFromJsonAsync<CdaTokenResponseModel>();
                result!.StatusCode = HttpStatusCode.OK;
            }

            else if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                var redirect = await response.Content.ReadFromJsonAsync<ClaimsGatheringResponseModel>();

                result = new CdaTokenResponseModel
                {
                    StatusCode = HttpStatusCode.Forbidden,
                    UserRedirectDetails = redirect
                };
            }
            else
            {
                result = new CdaTokenResponseModel
                {
                    StatusCode = response.StatusCode
                };
            }
        }
        catch(JsonException error)
        {
            throw new ServiceCommunicationException(Constants.TokenSerialisationError, error);
        }

        return result;
    }
}