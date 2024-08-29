using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CommonServices.Models.MessageBodyModels
{
    public class PensionRequestPayload
    {
        [JsonPropertyName("pensionRetrievalRecordId")]
        public string? PensionRetrievalRecordId { get; set; }

        [JsonPropertyName("pei")]
        public string? Pei { get; set; }

        [JsonPropertyName("iss")]
        public string? Iss { get; set; }

        [JsonPropertyName("userSessionId")]
        public string? UserSessionId { get; set; }

        [JsonPropertyName("asset_guid")]
        public string? AssetGuid { get; set; }

    }
}
