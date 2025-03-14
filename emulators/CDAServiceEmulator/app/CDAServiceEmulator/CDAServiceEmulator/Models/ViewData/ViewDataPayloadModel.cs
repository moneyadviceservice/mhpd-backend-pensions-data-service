using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace CDAServiceEmulator.Models.ViewData;

public class ViewDataPayloadModel
{
    [JsonProperty(PropertyName = "id")]
    public string? Id { get; set; }

    [JsonProperty(PropertyName = "assetGuid")]
    public string? AssetGuid { get; set; }

    [JsonProperty(PropertyName = "view_data")]
    public JObject? ViewData { get; set; }
}
