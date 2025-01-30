using MhpdCommon.Caching;
using MhpdCommon.Constants.HttpClient;
using MhpdCommon.Models.MHPDModels.JwkUri;
using Microsoft.Extensions.Logging;

namespace MhpdCommon.SharedHttpClient;

public class SharedHttpClient(IHttpClientFactory httpClientFactory, ILogger<SharedHttpClient> logger, 
    IJwkKeyCache<JwkUriResponseModel> jwkCache)
    : BaseHttpClientExecutor(httpClientFactory, logger), ISharedHttpClient
{
    private const string JwkCacheId = "C04701A6-2C39-4D0D-8BEC-58F66BBEC39F";

    public async Task<JwkUriResponseModel> GetAsync()
    {
        JwkUriResponseModel? response = null;

        try
        {
            response = await jwkCache.GetByIdAsync(JwkCacheId, JwkCacheId);
            logger.LogWarning("Found Jwk keys from cache. Skipping GET request...");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error reading Jwk keys from cache. Fetching from source...");
        }

        if (response != null)
        {
            return response;
        }

        response = await ExecuteAsync<JwkUriResponseModel>(HttpClientNames.CdaService,
            httpClient => httpClient.GetAsync(HttpEndpoints.External.JwkUri),
            HttpClientOperationName.CdaServiceJwkUriGet);

        response.Id = JwkCacheId;

        try
        {
            await jwkCache.InsertItemAsync(response, JwkCacheId);
            logger.LogWarning("Caching Jwk Keys from GET request...");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error saving Jwk keys to cache");
        }

        return response;
    }
}