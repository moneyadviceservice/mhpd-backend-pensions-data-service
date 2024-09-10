using CommonServices.Models;
using System.Text.Json.Serialization;

namespace MAPS.Core.Models.MessageBodyModels
{
    public class RetrievedPensionDetailsPayload
    {
        [JsonPropertyName("pensionRetrievalRecordId")]
        public string? PensionRetrievalRecordId { get; set; }

        [JsonPropertyName("pei")]
        public string? Pei { get; set; }

        [JsonPropertyName("retrievedPensionArrangements")]
        public List<PensionArrangement>? PensionArrangements { get; set; }

    }
}
