using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CDAServiceApiTests.Support
{
    public class ResponseDataModel
    {
        public class HolderConfigurationsList
        {
            [JsonProperty("holder_configurations")]
            public List<holder_configurations>? holder_configurations { get; set; }
        }

        public class holder_configurations
        {
            [JsonProperty("holdername_guid")]
            public string? holdername_guid { get; set; }

            [JsonProperty("veiw_data_url")]
            public string? veiw_data_url { get; set; }            
        }

        public class CdaPeiResponseBody
        {
            [JsonProperty("holdername_guid")]
            public string? holdername_guid { get; set; }

            [JsonProperty("veiw_data_url")]
            public string? veiw_data_url { get; set; }
        }
    }
}
