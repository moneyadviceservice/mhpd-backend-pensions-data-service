using MhpdCommon.Models.MessageBodyModels;

namespace PensionsRetrievalFunction.Repository;

public interface IPensionRetrievalRepository
{
    Task<bool> CreateRecordIfNotExistsAsync(PensionRetrievalPayload payload);
}
