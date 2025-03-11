using MhpdCommon.Caching;
using MhpdCommon.Constants;
using MhpdCommon.Constants.HttpClient;
using MhpdCommon.Models.MHPDModels;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace PensionRequestFunction.HttpClient;

public class HolderNameClient(IHttpClientFactory httpClientFactory,
    IHolderNameConfigurationCache<HolderNameConfigurationModel> cache,
    ILogger<HolderNameClient> logger) : IHolderNameClient
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly ILogger<HolderNameClient> _logger = logger;
    private readonly IHolderNameConfigurationCache<HolderNameConfigurationModel> _cache = cache;

    public async Task<HolderNameConfigurationModel?> GetViewDataUrlAsync(string holderNameId, string correlationId)
    {
        HolderNameConfigurationModel? cachedModel = null;

        try
        {
            cachedModel = await _cache.GetByIdStreamAsync(holderNameId, holderNameId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading holdername view configuration from cache. Fetching from source...");
        }

        if (cachedModel != null)
        {
            _logger.LogWarning("Cached version of holder name configuration found for {holderNameId}.", holderNameId);
            return cachedModel;
        }

        var client = _httpClientFactory.CreateClient(HttpClientNames.CdaService);

        client.DefaultRequestHeaders.Add(HeaderConstants.RequestId, Guid.NewGuid().ToString());
        client.DefaultRequestHeaders.Add(HeaderConstants.CorrelationId, correlationId);

        var response = await client.GetAsync($"{HttpEndpoints.External.HolderNameViewConfigurations}?{QueryParams.Cda.HolderName.Guid}={holderNameId}");

        response.EnsureSuccessStatusCode();

        var model = await CreateResponse(response);

        if (model != null)
        {
            try
            {
                await _cache.InsertItemAsync(model, model.HolderNameGuid!);
            }
            catch (Exception error)
            {
                _logger.LogError(error, "Error caching retrieved holdername view configuration");
            }
        }

        return model;
    }

    private async Task<HolderNameConfigurationModel?> CreateResponse(HttpResponseMessage? httpResponse)
    {
        var viewDataResponse = await httpResponse!.Content.ReadFromJsonAsync<HolderNameConfigurationModel>();

        if (viewDataResponse?.ViewDataUrl == null)
        {
            _logger.LogWarning("The holder name endpoint did not respond with exactly one configuration record");
            return null;
        }

        return viewDataResponse;
    }
}
