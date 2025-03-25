using MhpdCommon.Constants;
using MhpdCommon.Constants.HttpClient;
using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.Utils;
using System.Net;

namespace TokenIntegrationService.HttpClients;

public class CdaServiceClient(IHttpClientFactory httpClientFactory, ILogger<CdaServiceClient> logger) : ICdaServiceClient
{
    public async Task<CdaTokenResponseModel> PostAsync<TRequest>(TRequest request)
    {
        var httpClient = httpClientFactory.CreateClient(HttpClientNames.CdaService);
        var requestId = Guid.NewGuid().ToString();
        logger.LogWarning("Sending request to token endpoint with request Id: {RequestId}", requestId);

        httpClient.DefaultRequestHeaders.Add(HeaderConstants.RequestId, requestId);

        var payload = request switch
        {
            TokenClientRequestModel tokenRequest => UrlHelper.ConstructFormEncodedPayload(tokenRequest),
            CdaTokenRequestModel cdaTokenRequest => UrlHelper.ConstructFormEncodedPayload(cdaTokenRequest),
            _ => throw new InvalidOperationException("Unsupported request type.")
        };

        CdaTokenResponseModel? result = null;

        try
        {
            var response = await httpClient.PostAsync(HttpEndpoints.External.CdaTokenServiceEndpoint, payload);

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
        }
        catch(Exception error)
        {
            throw new InvalidOperationException("Unable to read from Cda Token response", error);
        }

        if(result == null)
        {
            throw new InvalidOperationException("Unable to get token or redirect details");
        }

        return result;
    }
}