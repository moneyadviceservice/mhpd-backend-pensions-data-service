using System.Text.Json.Serialization;

namespace MhpdCommon.Models.MHPDModels;

public class PensionArrangement
{
    [JsonPropertyName("externalAssetId")]
    public string? ExternalAssetId { get; set; }

    [JsonPropertyName("matchType")]
    public string? MatchType { get; set; }

    [JsonPropertyName("schemeName")]
    public string? SchemeName { get; set; }

    [JsonPropertyName("alternateSchemeNames")]
    public List<AlternateSchemeName>? AlternateSchemeNames { get; set; }

    [JsonPropertyName("contactReference")]
    public string? ContactReference { get; set; } = "";

    [JsonPropertyName("pensionAdministrator")]
    public PensionAdministrator? PensionAdministrator { get; set; }
}
