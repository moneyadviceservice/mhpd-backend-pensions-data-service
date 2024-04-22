using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeiIntegrationServiceApiTests.Support
{
    public class Parameters
    {
        public static string localHostEndpoint = "http://localhost:5218/peis";

        public static string localHostUri = "localhost";

        public static int portNo = 5218;

        public static string AuthorisationCode = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJ";

        public static string xRequestID = "1111-2222-3333-4444";

        public static string xVersion = "1.0";

        public static string ownerScope = "owner";

        public static string ummaScope = "uma_protection";

        public static string guid = "0d9b46c0-00fd-4f18-86b2-dfa0994c9ff3";

        public static string iss = "https://maps.com";

        public static string userSessionId = "askdj902139012ekasdlasdj";

        public static string azureBaseUrl = "https://peiintegratiionservice.azurewebsites.net/peis";

        public static string azureUrl = "peiintegratiionservice.azurewebsites.net/peis";

        public static string requestBodyRequestId = "qwertyuoip";

        public static string localHostRequestBodyPeisBaseUrl = "http://localhost:5089";

        public static string azureHostRequestBodyPeisBaseUrl = "https://cdaserviceemulator.azurewebsites.net";
    }
}
