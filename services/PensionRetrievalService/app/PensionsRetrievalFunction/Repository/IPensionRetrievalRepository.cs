using PensionsRetrievalFunction.Models;

namespace PensionsRetrievalFunction.Repository;

public interface IPensionRetrievalRepository
{
    Task<bool> CreateRecordIfNotExistsAsync(PensionRetrievalMessage retrievalMessage);
}
