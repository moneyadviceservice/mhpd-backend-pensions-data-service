using MhpdCommon.Models.MHPDModels;
using MhpdCommon.Models.RequestHeaderModel;
using PensionsDataService.Models;

namespace PensionsDataService.HttpClients;

public interface IRetrievedPensionsRecordClient
{
    Task<List<RetrievedPensionRecord>> GetAsync(PensionsRetrievalRecordIdModel request, RequestHeaderModel requestHeader);

    Task<int> DeleteAsync(string pensionsRetrievalRecordId, string correlationId);
}