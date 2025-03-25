using MhpdCommon.Constants.HttpClient;
using MhpdCommon.Models.MHPDModels;
using MhpdCommon.Models.RequestHeaderModel;
using Microsoft.Extensions.Logging;

namespace MhpdCommon.SharedHttpClient;

public class MapsCdaServiceClient(IHttpClientFactory httpClientFactory, ILogger<MapsCdaServiceClient> logger)
    : BaseHttpClientExecutor(httpClientFactory, logger), IMapsCdaServiceClient
{
    public async Task<MapsRqpServiceResponseModel> PostRqp(RequestHeaderModel request)
    { 
        return await ExecuteAsync<MapsRqpServiceResponseModel>(HttpClientNames.MapsCdaService,
            httpClient => httpClient.GetAsync(HttpEndpoints.Internal.Rqp),
            HttpClientOperationName.MapsCdaServiceRqp);
    }
}