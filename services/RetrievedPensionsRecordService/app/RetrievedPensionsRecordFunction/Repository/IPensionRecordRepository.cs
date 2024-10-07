using MhpdCommon.Models.MessageBodyModels;
using RetrievedPensionsRecordFunction.Models;

namespace RetrievedPensionsRecordFunction.Repository;

public interface IPensionRecordRepository
{
    Task<bool> SaveRetrievedPensionRecordAsync(string? correlationId, RetrievedPensionDetailsPayload payload);

    Task<List<RetrievedPensionRecord>> GetRetrievedRecordsAsync(string pensionsRetrievalRecordId);
}
