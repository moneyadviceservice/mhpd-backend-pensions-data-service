using PensionsRetrievalFunction.Models;

namespace PensionsRetrievalFunction.HttpClients;

public interface IPeiServiceClient
{
    Task<PeiDataResponse> GetPeiDataAsync(string? rpt, string? iss, string? peisId, string? userSessionId);
}
