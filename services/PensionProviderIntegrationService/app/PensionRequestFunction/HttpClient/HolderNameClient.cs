using MhpdCommon.Constants;
using MhpdCommon.Constants.HttpClient;
using MhpdCommon.Models.MHPDModels;
using Microsoft.Extensions.Logging;
using PensionRequestFunction.Repository;
using System.Net.Http.Json;

namespace PensionRequestFunction.HttpClient;

public class HolderNameClient(IHttpClientFactory httpClientFactory, 
    IHolderNameConfigurationRepository<HolderNameConfigurationModel> repository,
    ILogger<HolderNameClient> logger) : IHolderNameClient
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly ILogger<HolderNameClient> _logger = logger;
    private readonly IHolderNameConfigurationRepository<HolderNameConfigurationModel> _repository = repository;

    public async Task<HolderNameConfigurationModel?> GetViewDataUrlAsync(string holderNameId, string correlationId)
    {
        var cachedModel = await _repository.GetByIdStreamAsync(holderNameId, holderNameId);

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
                await _repository.InsertItemAsync(model, model.HolderNameGuid!);
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
        var viewDataResponse = await httpResponse!.Content.ReadFromJsonAsync<HolderNameViewDataResponse>();

        if (viewDataResponse == null || viewDataResponse.Configurations.Count != 1)
        {
            _logger.LogWarning("The holder name endpoint did not respond with exactly one configuration record");
            return null;
        }

        return viewDataResponse.Configurations.Single();
    }
}
