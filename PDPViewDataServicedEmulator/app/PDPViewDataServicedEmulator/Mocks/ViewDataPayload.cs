using Newtonsoft.Json;

namespace PDPViewDataServicedEmulator.Mocks
{
    public class ViewDataPayload
    {
        [JsonProperty(PropertyName = "assetGuid")]
        public string? AssetGuid { get; set; }

        [JsonProperty(PropertyName = "viewData")]
        public string? ViewData { get; set; }
    }
}
