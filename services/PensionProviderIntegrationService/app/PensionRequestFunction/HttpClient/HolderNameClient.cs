using MhpdCommon.Constants;
using MhpdCommon.Constants.HttpClient;
using MhpdCommon.Models.MHPDModels;
using System.Net.Http.Json;

namespace PensionRequestFunction.HttpClient;

public class HolderNameClient(IHttpClientFactory httpClientFactory) : IHolderNameClient
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

    public async Task<HolderNameConfigurationModel?> GetViewDataUrlAsync(string holderNameId)
    {
        var client = _httpClientFactory.CreateClient(HttpClientNames.CdaService);

        client.DefaultRequestHeaders.Add(HeaderConstants.RequestId, Guid.NewGuid().ToString());

        var response = await client.GetAsync($"{HttpEndpoints.External.HolderNameViewConfigurations}?{QueryParams.Cda.HolderName.Guid}={holderNameId}");

        response.EnsureSuccessStatusCode();

        return await CreateResponse(response);
    }

    private static async Task<HolderNameConfigurationModel?> CreateResponse(HttpResponseMessage? httpResponse)
    {
        if (httpResponse!.StatusCode != System.Net.HttpStatusCode.OK) return null;

        var viewDataResponse = await httpResponse!.Content.ReadFromJsonAsync<HolderNameViewDataResponse>();

        if (viewDataResponse == null || viewDataResponse.Configurations.Count != 1) return null;

        return viewDataResponse.Configurations.Single();
    }
}
