using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using PensionRequestFunction.HttpClient.Interfaces;
using PensionRequestFunction.Models.MapsRqpServiceClient;

namespace PensionRequestFunction.HttpClient.Implementation
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
            _mapsCdaServiceEndpoint = Environment.GetEnvironmentVariable("MapsCdaServiceEndpoint")!;
        }
        public async Task<MapsRqpServiceResponseModel> PostRqp(MapsRqpServiceRequestModel request)
        {
            var client = _httpClientFactory.CreateClient("MapsCdaService");
            client.BaseAddress = new Uri(_mapsCdaServiceEndpoint);

            var payload = JsonSerializer.Serialize(request);
            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            var endPoint = $"rqp";

            var responseMaPSCDA = await client!.PostAsync(endPoint, content);

            var result = await responseMaPSCDA.Content.ReadFromJsonAsync<MapsRqpServiceResponseModel>();

            return result!;
        }
    }
}
