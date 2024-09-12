using MhpdCommon.Models.MessageBodyModels;

namespace RetrievedPensionsRecordFunction.Repository;

public interface IPensionRecordRepository
{
    Task<bool> SaveRetrievedPensionRecordAsync(string? correlationId, RetrievedPensionDetailsPayload payload);
}
