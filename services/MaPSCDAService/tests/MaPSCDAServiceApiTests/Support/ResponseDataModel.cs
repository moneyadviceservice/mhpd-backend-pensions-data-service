using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MaPSCDAServiceApiTests.Support
{
    internal class ResponseDataModel
    {
        public class CdaTokenResponses
        {
            [JsonProperty("rqp")]
            public string? rqp { get; set; }            
        }        
    }
}
