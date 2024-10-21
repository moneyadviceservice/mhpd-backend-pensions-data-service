using MhpdCommon.Constants.HttpClient;
using PensionRequestFunction.HttpClient.Interfaces;
using PensionRequestFunction.Models.MapsRqpServiceClient;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace PensionRequestFunction.HttpClient.Implementation
{
    public class MapsCdaServiceClient(IHttpClientFactory httpClientFactory) : IMapsCdaServiceClient
    {
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

        public async Task<MapsRqpServiceResponseModel> PostRqpAsync(MapsRqpServiceRequestModel request)
        {
            var client = _httpClientFactory.CreateClient(HttpClientNames.MapsCdaService);

            var payload = JsonSerializer.Serialize(request);
            var content = new StringContent(payload, Encoding.UTF8, "application/json");

            var responseMaPSCDA = await client!.PostAsync(HttpEndpoints.Internal.Rqp, content);

            var result = await responseMaPSCDA.Content.ReadFromJsonAsync<MapsRqpServiceResponseModel>();

            return result!;
        }
    }
}
