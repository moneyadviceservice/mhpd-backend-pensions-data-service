using System.Text;
using System.Text.Json;
using PeiIntegrationService.HttpClients.Interfaces;
using PeiIntegrationService.Models;
using PeiIntegrationService.Models.MapsCdaService;

namespace PeiIntegrationService.HttpClients.Implementation
{
    public class MapsCdaServiceClient : IMapsRqpServiceClient
    {
        private readonly IConfiguration _configuration;
        private readonly string _mapsCdaServiceEndpoint;
        private readonly IHttpClientFactory _httpClientFactory;

        public MapsCdaServiceClient(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _mapsCdaServiceEndpoint = _configuration[Constants.HttpEndpoints.MapsCdaServiceEndpoint]!;
        }
        public async Task<MapsRqpServiceResponseModel> PostRqp(MapsRqpServiceRequestModel request)
        {
            var client = _httpClientFactory.CreateClient(Constants.HttpClients.MapsCdaService);
            client.BaseAddress = new Uri(_mapsCdaServiceEndpoint);

            var payload = JsonSerializer.Serialize(request);
            var content = new StringContent(payload, Encoding.UTF8, "application/json");

            var responseMaPSCDA = await client!.PostAsync(Constants.RequestRoutes.Rqp, content);

            var result = await responseMaPSCDA.Content.ReadFromJsonAsync<MapsRqpServiceResponseModel>();

            return result!;
        }
    }
}

