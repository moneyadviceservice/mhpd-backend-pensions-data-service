using System.Text.Json.Serialization;

namespace CommonServices.Models.MessageBodyModels
{
    public class RetrievedPensionDetailsPayload
    {
        [JsonPropertyName("pensionRetrievalRecordId")]
        public string? PensionRetrievalRecordId { get; set; }

        [JsonPropertyName("pei")]
        public string? Pei { get; set; }

        [JsonPropertyName("pensionArrangements")]
        public List<PensionArrangement>? PensionArrangements { get; set; }

    }
}
