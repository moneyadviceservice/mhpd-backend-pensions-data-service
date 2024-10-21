using System.Text.Json.Serialization;

namespace MhpdCommon.Models.MHPDModels;

public class ContactMethods
{
    [JsonPropertyName("preferred")]
    public bool Preferred { get; set; }

    [JsonPropertyName("contactMethodDetails")]
    public ContactMethodDetails? ContactMethodDetails { get; set; }
}

public class ContactMethodDetails
{
    [JsonPropertyName("email")]
    public string? Email { get; set; } = "";

    [JsonPropertyName("number")]
    public string? Number { get; set; } = "";

    [JsonPropertyName("usage")]
    public List<string>? Usage { get; set; } = new List<string>();

    [JsonPropertyName("url")]
    public string? Url { get; set; } = "";

}
