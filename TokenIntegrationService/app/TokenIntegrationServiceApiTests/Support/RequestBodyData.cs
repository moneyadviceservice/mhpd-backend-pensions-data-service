using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TokenIntegrationServiceApiTests.Support
{
    internal class RequestBodyData
    {
        public string? Ticket { get; set; }
        public string? Rqp { get; set; }

        public string? AsUri { get; set; }
    }
}
