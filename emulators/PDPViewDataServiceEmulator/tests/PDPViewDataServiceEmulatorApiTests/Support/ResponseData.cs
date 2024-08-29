using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PDPViewDataServiceEmulatorApiTests.Support
{
    internal class ResponseData
    {
        public class ViewDataResponse
        {
            [JsonProperty("view_data_token")]
            public string? view_data_token { get; set; }            
        }
    }
}
