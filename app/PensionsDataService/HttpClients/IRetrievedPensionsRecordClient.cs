using MhpdCommon.Models.MHPDModels;
using MhpdCommon.Models.RequestHeaderModel;
using PensionsDataService.Models;

namespace PensionsDataService.HttpClients;

public interface IRetrievedPensionsRecordClient
{
    Task<List<RetrievedPensionRecord>> GetRetrievedPensionsAsync(RetrievedPensionsRequest request, RequestHeaderModel requestHeader);

    Task<List<string>> GetRetrievedPeisAsync(RequestHeaderModel requestHeader);

    Task<int> DeleteAsync(string userSessionId, string correlationId);
}