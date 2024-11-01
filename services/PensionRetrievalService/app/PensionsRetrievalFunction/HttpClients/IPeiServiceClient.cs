using PensionsRetrievalFunction.Models;

namespace PensionsRetrievalFunction.HttpClients;

public interface IPeiServiceClient
{
    Task<PeiDataResponse> GetPeiDataAsync(PeiRequest request);
}
