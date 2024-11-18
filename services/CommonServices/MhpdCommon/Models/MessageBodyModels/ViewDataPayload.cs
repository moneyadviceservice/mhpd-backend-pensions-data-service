using System.Text.Json.Serialization;

namespace MhpdCommon.Models.MessageBodyModels;

public class ViewDataPayload
{
    [JsonPropertyName("pensionLink")]
    public string? PensionLink { get; set; }

    [JsonPropertyName("arrangements")]
    public dynamic? Arrangements { get; set; }
}
