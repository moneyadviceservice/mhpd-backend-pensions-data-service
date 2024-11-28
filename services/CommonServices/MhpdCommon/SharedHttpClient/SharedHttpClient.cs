using MhpdCommon.Constants.HttpClient;
using MhpdCommon.Models.MHPDModels.JwkUri;
using Microsoft.Extensions.Logging;

namespace MhpdCommon.SharedHttpClient;

public class SharedHttpClient(IHttpClientFactory httpClientFactory, ILogger<SharedHttpClient> logger)
    : BaseHttpClientExecutor(httpClientFactory, logger), ISharedHttpClient
{
    public async Task<JwkUriResponseModel> GetAsync()
    {
        return await ExecuteAsync<JwkUriResponseModel>(HttpClientNames.CdaService,
            httpClient => httpClient.GetAsync(HttpEndpoints.External.JwkUri),
            HttpClientOperationName.CdaServiceJwkUriGet);
    }
}