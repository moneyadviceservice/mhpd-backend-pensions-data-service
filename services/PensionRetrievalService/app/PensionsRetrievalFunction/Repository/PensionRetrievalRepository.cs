using PensionsRetrievalFunction.Models;

namespace PensionsRetrievalFunction.Repository;

public class PensionRetrievalRepository : IPensionRetrievalRepository
{
    public Task<bool> CreateRecordIfNotExistsAsync(PensionRetrievalMessage retrievalMessage)
    {
        return Task.FromResult(false);
    }
}
