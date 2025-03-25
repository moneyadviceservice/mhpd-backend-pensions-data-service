using MhpdCommon.Constants.HttpClient;
using MhpdCommon.Models.MHPDModels;
using MhpdCommon.Models.RequestHeaderModel;
using Microsoft.Extensions.Logging;

namespace MhpdCommon.SharedHttpClient;

public class MapsCdaServiceClient(IHttpClientFactory httpClientFactory, ILogger<MapsCdaServiceClient> logger)
    : BaseHttpClientExecutor(httpClientFactory, logger), IMapsCdaServiceClient
{
    public async Task<MapsRqpServiceResponseModel> GetRqp(RequestHeaderModel requestHeaderModel)
    { 
        return await ExecuteAsync<MapsRqpServiceResponseModel>(
            HttpClientNames.MapsCdaService,
            _ => new HttpRequestMessage(HttpMethod.Get, HttpEndpoints.Internal.Rqp), // Creates the request
            HttpClientOperationName.MapsCdaServiceRqp,
            request => 
            {
                request.Headers.Add("mhpdCorrelationId", requestHeaderModel.CorrelationId);
                request.Headers.Add("userSessionId", requestHeaderModel.UserSessionId);
                request.Headers.Add("iss", requestHeaderModel.Iss);
            });
    }
}