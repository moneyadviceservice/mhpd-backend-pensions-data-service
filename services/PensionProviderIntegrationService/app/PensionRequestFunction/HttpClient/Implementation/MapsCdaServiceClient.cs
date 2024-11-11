using MhpdCommon.Constants;
using MhpdCommon.Constants.HttpClient;
using MhpdCommon.Extensions;
using Microsoft.Extensions.Logging;
using PensionRequestFunction.HttpClient.Interfaces;
using PensionRequestFunction.Models.MapsRqpServiceClient;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace PensionRequestFunction.HttpClient.Implementation
{
    public class MapsCdaServiceClient(IHttpClientFactory httpClientFactory, ILogger<MapsCdaServiceClient> logger) : IMapsCdaServiceClient
    {
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
        private readonly ILogger<MapsCdaServiceClient> _logger = logger;

        public async Task<MapsRqpServiceResponseModel> PostRqpAsync(MapsRqpServiceRequestModel request)
        {
            _logger.LogRequest(request);
            var client = _httpClientFactory.CreateClient(HttpClientNames.MapsCdaService);
            client.DefaultRequestHeaders.Add(HeaderConstants.CorrelationId, request.CorrelationId);

            var payload = JsonSerializer.Serialize(request);
            var content = new StringContent(payload, Encoding.UTF8, "application/json");

            var responseMaPSCDA = await client!.PostAsync(HttpEndpoints.Internal.Rqp, content);

            var result = await responseMaPSCDA.Content.ReadFromJsonAsync<MapsRqpServiceResponseModel>();
            _logger.LogResponse(result);

            return result!;
        }
    }
}
