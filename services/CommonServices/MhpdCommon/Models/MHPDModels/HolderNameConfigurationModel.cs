using System.Text.Json.Serialization;

namespace MhpdCommon.Models.MHPDModels;

public class HolderNameConfigurationModel
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("holdername_guid")]
    public string? HolderNameGuid { get; set; }

    [JsonPropertyName("view_data_url")]
    public string? ViewDataUrl { get; set; }
}