using MhpdCommon.Models.MHPDModels;
using PensionsDataService.Models;

namespace PensionsDataService.HttpClients;

public interface IRetrievedPensionsRecordClient
{
    Task<List<RetrievedPensionRecord>> GetAsync(PensionsRetrievalRecordIdModel request);
}