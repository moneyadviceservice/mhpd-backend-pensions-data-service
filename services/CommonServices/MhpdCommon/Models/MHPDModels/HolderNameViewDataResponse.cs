using System.Text.Json.Serialization;

namespace MhpdCommon.Models.MHPDModels;

public class HolderNameViewDataResponse
{
    [JsonPropertyName("holder_view_configurations")]
    public List<HolderNameConfigurationModel> Configurations { get; set; } = [];
}
