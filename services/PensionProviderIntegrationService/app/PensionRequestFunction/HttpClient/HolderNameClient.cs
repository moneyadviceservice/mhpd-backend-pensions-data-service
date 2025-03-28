using MhpdCommon.Caching;
using MhpdCommon.Constants;
using MhpdCommon.Constants.HttpClient;
using MhpdCommon.Models.MHPDModels;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace PensionRequestFunction.HttpClient;

public class HolderNameClient(IHttpClientFactory httpClientFactory,
    IHolderNameConfigurationCache<HolderNameViewDataResponse> cache,
    ILogger<HolderNameClient> logger) : IHolderNameClient
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

        var client = httpClientFactory.CreateClient(HttpClientNames.CdaService);

        client.DefaultRequestHeaders.Add(HeaderConstants.RequestId, Guid.NewGuid().ToString());
        client.DefaultRequestHeaders.Add(HeaderConstants.CorrelationId, correlationId);

        var response = await client.GetAsync($"{HttpEndpoints.External.HolderNameViewConfigurations}?{QueryParams.Cda.HolderName.Guid}={holderNameId}");

        response.EnsureSuccessStatusCode();

        var model = await CreateResponse(response);

        if (model != null)
        {
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
        }

        return model;
    }

    private async Task<HolderNameViewDataResponse?> CreateResponse(HttpResponseMessage? httpResponse)
    {
        var viewDataResponse = await httpResponse!.Content.ReadFromJsonAsync<HolderNameViewDataResponse>();

        if (viewDataResponse?.Configuration.ViewDataUrl == null)
        {
            logger.LogWarning("No ViewDataUrl returned on the holder name endpoint response");
            return null;
        }

        return viewDataResponse;
    }
}
