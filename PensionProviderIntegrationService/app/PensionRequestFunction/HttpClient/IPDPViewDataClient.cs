namespace PensionRequestFunction.HttpClient
{
    public interface IPDPViewDataClient
    {
        Task<string> GetPDPViewDataAsync(string assetGuid, string viewDataUrl, string rpt);
    }
}
