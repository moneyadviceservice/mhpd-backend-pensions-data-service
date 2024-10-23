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

    public async Task<HolderNameConfigurationModel?> GetViewDataUrlAsync(string holderNameId)
    {
        var cachedModel = await _repository.GetByIdAsync(holderNameId, holderNameId);
        if (cachedModel != null) return cachedModel;

        var client = _httpClientFactory.CreateClient(HttpClientNames.CdaService);

        client.DefaultRequestHeaders.Add(HeaderConstants.RequestId, Guid.NewGuid().ToString());

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

    private static async Task<HolderNameConfigurationModel?> CreateResponse(HttpResponseMessage? httpResponse)
    {
        if (httpResponse!.StatusCode != System.Net.HttpStatusCode.OK) return null;

        var viewDataResponse = await httpResponse!.Content.ReadFromJsonAsync<HolderNameViewDataResponse>();

        if (viewDataResponse == null || viewDataResponse.Configurations.Count != 1) return null;

        return viewDataResponse.Configurations.Single();
    }
}
