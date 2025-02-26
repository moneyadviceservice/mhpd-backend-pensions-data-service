using System.Text.Json.Serialization;

namespace MhpdCommon.Models.MHPDModels;

public class HolderNameConfigurationModel
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("holdernameGuid")]
    public string? HolderNameGuid { get; set; }

    [JsonPropertyName("view_data_host_url")]
    public string? ViewDataUrl { get; set; }
}