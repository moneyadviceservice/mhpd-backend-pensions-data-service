using MhpdCommon.Models.MessageBodyModels;
using PensionsRetrievalFunction.Models;

namespace PensionsRetrievalFunction.Repository;

public interface IPensionRetrievalRepository
{
    Task<PensionsRetrievalRecord?> CreateRecordIfNotExistsAsync(PensionRetrievalPayload payload);

    Task UpdatePensionsRetrievalRecordAsync(PensionsRetrievalRecord record);

    Task<PensionsRetrievalRecord?> GetRetrievalRecordAsync(string userSessionId);
}
