using System.Text.Json.Serialization;

namespace MhpdCommon.Models.MHPDModels;

public class HolderNameViewDataResponse
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("holdernameGuid")]
    public string? HolderNameGuid { get; set; }
    
    [JsonPropertyName("holderConfiguration")]
    public required HolderNameConfigurationModel Configuration { get; set; }
}
