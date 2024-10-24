using MhpdCommon.Models.MHPDModels;
using MhpdCommon.Models.RequestHeaderModel;

namespace PensionsDataService.HttpClients;

public interface IRetrievalRecordServiceClient
{
    Task<PensionsRetrievalRecord> GetAsync(RequestHeaderModel requestHeader);
}