using System.Text;
using System.Text.Json;
using MhpdCommon.Constants;
using MhpdCommon.Constants.HttpClient;
using MhpdCommon.Extensions;
using PeiIntegrationService.HttpClients.Interfaces;
using PeiIntegrationService.Models.MapsCdaService;

namespace PeiIntegrationService.HttpClients.Implementation;

public class MapsCdaServiceClient(IHttpClientFactory httpClientFactory, ILogger<MapsCdaServiceClient> logger) : IMapsRqpServiceClient
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly ILogger<MapsCdaServiceClient> _logger = logger;

    public async Task<MapsRqpServiceResponseModel> PostRqp(MapsRqpServiceRequestModel request)
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

