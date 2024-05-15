using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TokenIntegrationServiceApiTests.Support
{
    internal class ResponseDataModel
    {
        public class RptTokenResponses
        {
            [JsonProperty("rpt")]
            public string? rpt { get; set; }            
        }
    }
}
