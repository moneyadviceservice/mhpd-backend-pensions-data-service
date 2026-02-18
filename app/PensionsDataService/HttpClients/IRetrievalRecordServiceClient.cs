using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.Models.MHPDModels;
using MhpdCommon.Models.RequestHeaderModel;

namespace PensionsDataService.HttpClients;

public interface IRetrievalRecordServiceClient
{
    Task<PensionsRetrievalRecord> PostAsync(RequestHeaderModel requestHeader, PensionRetrievalPayload payload);

    Task<PensionsRetrievalRecord> GetAsync(RequestHeaderModel requestHeader);

    Task DeleteAsync(string userSessionId, string correlationId);
}