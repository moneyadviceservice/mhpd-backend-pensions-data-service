using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDATokenServicesApiTests.Support
{
    internal class Parameters
    {
        public static string localHostEndpoint = "http://localhost:5089/peis/1111-2222-3333-4444";

        public static string AuthorisationCode = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJ";

        public static string xRequestID = "sdfasdfasdasdadsg";

        public static string xVersion = "1.0";

        public static string ownerScope = "owner";

        public static string ummaScope = "uma_protection";

        public static string guid = "0d9b46c0-00fd-4f18-86b2-dfa0994c9ff3";

        public static string azureCdaEmulatorUrl = "cdaserviceemulator.azurewebsites.net/peis/cd0e4fdc-8586-4483-9899-17dd85af9074";

        public static string azureCdaEmulatorBaseUrl = "cdaserviceemulator.azurewebsites.net";

        internal static string requestQuery = "grant_type=urn:ietf:params:oauth:grant-type:uma-ticket&ticket=eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c.&claim_token_format=pension_dashboad_rqp&claim_token=eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c&scope=owner";

        public static string azureTokenServiceUrl = "cdatokenservices.azurewebsites.net/token";

        public static string azureTokenServiceBaseUrl = "cdatokenservices.azurewebsites.net";

        public static string grantType = "urn:ietf:params:oauth:grant-type:uma-ticket";

        public static string ticketNo = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c.";
    }
}
