using System.Text.Json.Serialization;

namespace MhpdCommon.Models.MHPDModels;

public class HolderNameConfigurationModel
{
    [JsonPropertyName("view_data_host_url")]
    public string? ViewDataUrl { get; set; }
}