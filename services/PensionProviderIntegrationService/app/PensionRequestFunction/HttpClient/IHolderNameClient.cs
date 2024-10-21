using MhpdCommon.Models.MHPDModels;

namespace PensionRequestFunction.HttpClient;

public interface IHolderNameClient
{
    Task<HolderNameConfigurationModel?> GetViewDataUrlAsync(string holderNameId);
}
