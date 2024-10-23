using System.Text;
using System.Text.Json;
using MhpdCommon.Constants.HttpClient;
using PeiIntegrationService.HttpClients.Interfaces;
using PeiIntegrationService.Models.MapsCdaService;

namespace PeiIntegrationService.HttpClients.Implementation;

public class MapsCdaServiceClient(IHttpClientFactory httpClientFactory) : IMapsRqpServiceClient
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

    public async Task<MapsRqpServiceResponseModel> PostRqp(MapsRqpServiceRequestModel request)
    {
        var client = _httpClientFactory.CreateClient(HttpClientNames.MapsCdaService);

        var payload = JsonSerializer.Serialize(request);
        var content = new StringContent(payload, Encoding.UTF8, "application/json");

        var responseMaPSCDA = await client!.PostAsync(HttpEndpoints.Internal.Rqp, content);

        var result = await responseMaPSCDA.Content.ReadFromJsonAsync<MapsRqpServiceResponseModel>();

        return result!;
    }
}

