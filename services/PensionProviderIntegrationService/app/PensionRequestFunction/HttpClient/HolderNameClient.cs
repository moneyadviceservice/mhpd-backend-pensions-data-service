using MhpdCommon.Caching;
using MhpdCommon.Constants;
using MhpdCommon.Constants.HttpClient;
using MhpdCommon.Models.MHPDModels;
using MhpdCommon.SharedHttpClient;
using Microsoft.Extensions.Logging;

namespace PensionRequestFunction.HttpClient;

public class HolderNameClient(IHttpClientFactory httpClientFactory,
    IHolderNameConfigurationCache<HolderNameViewDataResponse> cache,
    ILogger<HolderNameClient> logger) 
    : BaseHttpClientExecutor(httpClientFactory, logger), IHolderNameClient
{
    public async Task<HolderNameViewDataResponse?> GetViewDataUrlAsync(string holderNameId, string correlationId)
    {
        HolderNameViewDataResponse? cachedModel = null;

        try
        {
            cachedModel = await cache.GetByIdStreamAsync(holderNameId, holderNameId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error reading holdername view configuration from cache. Fetching from source...");
        }

        if (cachedModel != null)
        {
            logger.LogWarning("Cached version of holder name configuration found for {HolderNameId}", holderNameId);
            return cachedModel;
        }

        var model = await ExecuteAsync<HolderNameViewDataResponse>(HttpClientNames.CdaService,
            _ => new HttpRequestMessage(HttpMethod.Get, $"{HttpEndpoints.External.HolderNameViewConfigurations}?{QueryParams.Cda.HolderName.Guid}={holderNameId}"),
            HttpClientOperationName.CdaServiceHolderNameGet,
            message =>
            {
                message.Headers.Add(HeaderConstants.RequestId, Guid.NewGuid().ToString());
                message.Headers.Add(HeaderConstants.CorrelationId, correlationId);
            });

        if (model?.Configuration.ViewDataUrl == null)
        {
            logger.LogWarning("No ViewDataUrl returned on the holder name endpoint response");
            return null;
        }

        try
        {
            model.Id = holderNameId;
            model.HolderNameGuid = holderNameId;
            await cache.InsertItemAsync(model, model.HolderNameGuid!);
        }
        catch (Exception error)
        {
            logger.LogError(error, "Error caching retrieved holdername view configuration");
        }

        return model;
    }
}
