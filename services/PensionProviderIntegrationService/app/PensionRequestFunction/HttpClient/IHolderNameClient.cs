using MhpdCommon.Models.MHPDModels;

namespace PensionRequestFunction.HttpClient;

public interface IHolderNameClient
{
    Task<HolderNameViewDataResponse?> GetViewDataUrlAsync(string holderNameId, string correlationId);
}
