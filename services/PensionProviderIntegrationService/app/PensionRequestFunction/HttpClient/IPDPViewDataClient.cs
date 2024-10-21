using PensionRequestFunction.Models.CdaPeisServiceClient;

namespace PensionRequestFunction.HttpClient
{
    public interface IPdpViewDataClient
    {
       Task<PdpServiceResponseModel> GetPdpViewDataAsync(string assetGuid, string viewDataUrl, string? rpt);
    }
}
