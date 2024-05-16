using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TokenIntegrationServiceApiTests.Support
{
    internal class RequestBodyData
    {
        public string? ticket { get; set; }
        public string? rqp { get; set; }
        public string? as_uri { get; set; }
    }
}
