using System.Text.Json.Serialization;

namespace MhpdCommon.Models.MHPDModels;

public class PensionAdministrator
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("contactMethods")]
    public List<ContactMethods>? ContactMethods { get; set; }
}