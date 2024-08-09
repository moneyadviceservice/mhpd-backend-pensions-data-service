using PensionRequestFunction.Models.CdaPeisServiceClient;

namespace PensionRequestFunction.HttpClient
{
    public interface IPDPViewDataClient
    {
       Task<PDPServiceResponseModel> GetPDPViewDataAsync(string assetGuid, string viewDataUrl, string rpt);
    }
}
