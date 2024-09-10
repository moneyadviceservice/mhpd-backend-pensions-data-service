using System.Text.Json.Serialization;

namespace MhpdCommon.Models.MHPDModels;

public class AlternateSchemeName
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("alternateNameType")]
    public string? AlternateNameType { get; set; }
}