using MhpdCommon.Constants.HttpClient;
using MhpdCommon.ErrorHandling;
using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.Models.MHPDModels;
using MhpdCommon.Models.RequestHeaderModel;
using PensionsDataService.Models;

namespace PensionsDataService.HttpClients;

public class RetrievalRecordServiceClient(IHttpClientFactory httpClientFactory, ILogger<RetrievalRecordServiceClient> logger, IErrorResolver errorResolver) 
    : PensionDataClient(httpClientFactory, errorResolver, logger), IRetrievalRecordServiceClient
{
    protected override string ClientName => "Retrieval Service Client";

    public async Task DeleteAsync(RequestHeaderModel requestHeader)
    {
        logger.LogWarning("Sending request to delete pensions retrieval records for session {Session}", requestHeader.UserSessionId);

        await ExecuteAsync(requestHeader, () => new HttpRequestMessage(HttpMethod.Delete, HttpEndpoints.Internal.PensionsRetrievalRecords), 
            HttpClientNames.PensionRetrievalService, Constants.HttpDelete);
    }

    public async Task<PensionsRetrievalRecord> PostAsync(RequestHeaderModel requestHeader, PensionRetrievalPayload payload)
    {
        return await ExecuteAsync<PensionsRetrievalRecord>(requestHeader, () => {
            return new HttpRequestMessage(HttpMethod.Post, HttpEndpoints.Internal.PensionsRetrievalRecords)
            {
                Content = JsonContent.Create(payload)
            };},
            HttpClientNames.PensionRetrievalService, Constants.HttpPost);
    }

    public async Task<PensionsRetrievalRecord> GetAsync(RequestHeaderModel requestHeader)
    {
        return await ExecuteAsync<PensionsRetrievalRecord>(requestHeader, () => new HttpRequestMessage(HttpMethod.Get, HttpEndpoints.Internal.PensionsRetrievalRecords),
            HttpClientNames.PensionRetrievalService, Constants.HttpGet);
    }
}
