using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDAServiceApiTests.Support
{
    public class QueryRequest
    {
        public QueryRequest(UriBuilder uriBuilder)
        {
            this.uriBuilder = uriBuilder;
        }

        public string? xRequestID { get; set; }

        public string? xVersion { get; set; }
        
        public UriBuilder uriBuilder { get; set; }
    }
}
