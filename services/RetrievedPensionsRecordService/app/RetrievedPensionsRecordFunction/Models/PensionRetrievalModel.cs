using System.Text.Json.Serialization;

namespace RetrievedPensionsRecordFunction.Models;

public class RetrievedPensionRecord
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    [JsonPropertyName("correlationId")]
    public string CorrelationId { get; set; } = string.Empty;
    [JsonPropertyName("pei")]
    public string? Pei { get; set; }
    [JsonPropertyName("pensionsRetrievalRecordId")]
    public string? PensionsRetrievalRecordId { get; set; }
    [JsonPropertyName("retrievalResult")]
    public RetrievalResult? RetrievalResult { get; set; }
}
