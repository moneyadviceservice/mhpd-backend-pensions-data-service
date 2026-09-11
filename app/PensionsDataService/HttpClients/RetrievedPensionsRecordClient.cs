using MhpdCommon.Constants.HttpClient;
using MhpdCommon.ErrorHandling;
using MhpdCommon.Extensions;
using MhpdCommon.Models.MHPDModels;
using MhpdCommon.Models.RequestHeaderModel;
using MhpdCommon.Utils;
using PensionsDataService.Models;

namespace PensionsDataService.HttpClients;

public class RetrievedPensionsRecordClient(IHttpClientFactory httpClientFactory, ILogger<RetrievedPensionsRecordClient> logger, IErrorResolver errorResolver) 
    : PensionDataClient(httpClientFactory, errorResolver, logger), IRetrievedPensionsRecordClient
{
    protected override string ClientName => "Retrieved Service Client";

    public async Task DeleteAsync(RequestHeaderModel requestHeader)
    {
        logger.LogWarning("Sending request to delete retrieved pension records for session {Session}", requestHeader.UserSessionId);

        await ExecuteAsync(requestHeader, () => new HttpRequestMessage(HttpMethod.Delete, HttpEndpoints.Internal.RetrievedPensionRecords),
            HttpClientNames.RetrievedPensionsService, Constants.HttpDelete);
    }

    public async Task<List<RetrievedPensionRecord>> GetRetrievedPensionsAsync(RetrievedPensionsRequest request, RequestHeaderModel requestHeader)
    {
        logger.LogRequestReceived(request);

        var path = HttpEndpoints.Internal.RetrievedPensionRecords;

        if (!string.IsNullOrWhiteSpace(request.AssetId) ||
            !string.IsNullOrWhiteSpace(request.Category))
        {
            path = UrlHelper.ConstructEndPoint(request, HttpEndpoints.Internal.RetrievedPensionRecords);
        }

        return await ExecuteAsync<List<RetrievedPensionRecord>>(requestHeader, () => new HttpRequestMessage(HttpMethod.Get, path),
            HttpClientNames.RetrievedPensionsService, Constants.HttpGet);
    }

    public async Task<List<string>> GetRetrievedPeisAsync(RequestHeaderModel requestHeader)
    {
        return await ExecuteAsync<List<string>>(requestHeader, () => new HttpRequestMessage(HttpMethod.Get, HttpEndpoints.Internal.RetrievedPeis),
            HttpClientNames.RetrievedPensionsService, $"{Constants.HttpGet} Peis");
    }
}