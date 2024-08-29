using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TokenIntegrationServiceApiTests.Support
{
    internal class Parameters
    {
        public static string xRequestID = "sdfasdfasdasdadsg";
        
        internal static string requestQuery = "grant_type=urn:ietf:params:oauth:grant-type:uma-ticket&ticket=eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c.&claim_token_format=pension_dashboad_rqp&claim_token=eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c&scope=owner";

        public static string azureTokenServiceUrl = "cdatokenservices.azurewebsites.net/token";

        public static string azureTokenServiceBaseUrl = "https://cdatokenservices.azurewebsites.net";        

        public static string ticketNo = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";

        public static string rqp = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";

        public static string localHostAsUri = "http://localhost:5044";

        public static string azureAsUri = "https://tokenintegrationservice.azurewebsites.net/rpts";

        public static string responseRpt = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJsb2dnZWRJbkFzIjoiYWRtaW4iLCJpYXQiOjE0MjI3Nzk2Mzh9.gzSraSYS8EXBxLN_oWnFSRgCzcmJmMjLiuyu5CSpyHI";

        public static string azureTokenIntegrationServiceUrl = "tokenintegrationservice.azurewebsites.net/rpts";

        public static string localHostTokenIntegrationServiceUrl = "localhost";

        public static int localHostPortNo = 5289;

    }
}
