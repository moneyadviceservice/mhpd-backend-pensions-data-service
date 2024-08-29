using Newtonsoft.Json;

namespace PeiIntegrationServiceApiTests.Support
{
    public class PeiResponseDataModel
    {
        public class PeiResponseBody
        {
            [JsonProperty("PeiResponses")]
            public PeiResponses? PeiResponses { get; set; }
        }

        public class PeiResponses
        {
            [JsonProperty("pei")]
            public string? pei { get; set; }

            [JsonProperty("description")]
            public string? description { get; set; }

            [JsonProperty("retrievalStatus")]
            public string? retrievalStatus { get; set; }

            [JsonProperty("retrievalRequestedTimestamp")]
            public DateTime? retrievalRequestedTimestamp { get; set; }
        }

        public class PeiResponseHeader
        {
            [JsonProperty("rpt")]
            public string? rpt { get; set; }            
        }

        public class ResponseHeaders
        {
            [JsonProperty("Date")]
            public DateTime? Date { get; set; }

            [JsonProperty("Server")]
            public string? Server { get; set; }

            [JsonProperty("TransferEncoding")]
            public string? TransferEncoding { get; set; }

            [JsonProperty("rpt")]
            public string? rpt { get; set; }

        }
    }
}
