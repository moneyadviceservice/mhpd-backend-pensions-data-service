using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.Models.MHPDModels;

namespace RetrievedPensionsRecordFunction.Repository;

public interface IPensionRecordRepository
{
    Task<bool> SaveRetrievedPensionRecordAsync(string? correlationId, RetrievedPensionDetailsPayload payload);

    Task<List<RetrievedPensionRecord>> GetRetrievedRecordsAsync(string pensionsRetrievalRecordId);
}
