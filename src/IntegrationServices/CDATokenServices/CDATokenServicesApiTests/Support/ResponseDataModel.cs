using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CDATokenServicesApiTests.Support
{
    internal class ResponseDataModel
    {
        public class CdaTokenResponses
        {
            [JsonProperty("access_token")]
            public string? access_token { get; set; }

            [JsonProperty("token_type")]
            public string? token_type { get; set; }

            [JsonProperty("upgraded")]
            public bool upgraded { get; set; }

            [JsonProperty("pct")]
            public string? pct { get; set; }
        }
    }
}
