using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PDPViewDataServicedEmulator.Mocks
{
    public class ViewDataPayload
    {
        [JsonProperty(PropertyName = "assetGuid")]
        public string? AssetGuid { get; set; }

        [JsonProperty(PropertyName = "view_data")]
        public JObject? ViewData { get; set; }
    }
}
