using System.Text.Json.Serialization;

namespace CommonServices.Models
{
    public class RetrievedPensionDataModel
    {
        [JsonPropertyName("pensionRetrievalRecordId")]
        public string? PensionRetrievalRecordId { get; set; }

        [JsonPropertyName("pei")]
        public string? Pei { get; set; }

        [JsonPropertyName("pensionArrangement")]
        public List<PensionArrangement>? PensionArrangements { get; set; }

    }
}
