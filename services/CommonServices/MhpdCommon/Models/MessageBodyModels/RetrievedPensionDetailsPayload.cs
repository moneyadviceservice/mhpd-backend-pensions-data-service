using System.Text.Json.Serialization;

namespace MhpdCommon.Models.MessageBodyModels;

public class RetrievedPensionDetailsPayload
{
    [JsonPropertyName("pensionRetrievalRecordId")]
    public string? PensionRetrievalRecordId { get; set; }

    [JsonPropertyName("pei")]
    public string? Pei { get; set; }

    [JsonPropertyName("retrievalResult")]
    public dynamic? RetrievalResult { get; set; }
}
