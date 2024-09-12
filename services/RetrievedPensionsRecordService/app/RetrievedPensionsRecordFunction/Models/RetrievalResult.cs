using MhpdCommon.Models.MHPDModels;
using System.Text.Json.Serialization;

namespace RetrievedPensionsRecordFunction.Models
{
    public class RetrievalResult
    {
        [JsonPropertyName("retrievedPensionArrangements")]
        public List<PensionArrangement>? PensionArrangements { get; set; }
    }
}
