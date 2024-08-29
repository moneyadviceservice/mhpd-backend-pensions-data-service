using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDAServiceApiTests.Support
{
    internal class Parameters
    {
        public static string localHostEndpoint = "http://localhost:5089/peis/1111-2222-3333-4444";

        public static string AuthorisationCode = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJsb2dnZWRJbkFzIjoiYWRtaW4iLCJpYXQiOjE0MjI3Nzk2Mzh9.gzSraSYS8EXBxLN_oWnFSRgCzcmJmMjLiuyu5CSpyHI";

        public static string xRequestID = "b7301d11-f166-499a-9bf1-0598c2f1af52";

        public static string xVersion = "1.0";

        public static string ownerScope = "owner";

        public static string ummaScope = "uma_protection";

        public static string guid = "0d9b46c0-00fd-4f18-86b2-dfa0994c9ff3";

        public static string azureCdaEmulatorPeiUrl = "cdaserviceemulator.azurewebsites.net/peis/cd0e4fdc-8586-4483-9899-17dd85af9074";

        public static string azureCdaEmulatorPeiUrlWithoutPeisId = "cdaserviceemulator.azurewebsites.net/peis/";

        public static string azureCdaEmulatorHolderNameConfigurationsUrl = "cdaserviceemulator.azurewebsites.net/holdername-configurations";
        
        public static string azureCdaEmulatorTokenConfigurationsUrl = "cdaserviceemulator.azurewebsites.net/token";

        public static string cdaServiceEmulatorBaseUrl = "cdaserviceemulator.azurewebsites.net/";

        public static string firstElementGuid = "7075aa11-10ad-4b2f-a9f5-1068e79119bf";

        public static string firstElementViewDataUrl = "https://local.exampleprovider/pensiondataprovider";

        public static string secondElementGuid = "550e8400-e29b-41d4-a716-446655440000";

        public static string secondElementViewDataUrl = "https://local.exampleprovider2/pensiondataprovider";
        
        internal static string tokenRequestQuery = "grant_type=urn:ietf:params:oauth:grant-type:uma-ticket&ticket=eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c.&claim_token_format=pension_dashboad_rqp&claim_token=eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c&scope=owner";
        
        public static string grantType = "urn:ietf:params:oauth:grant-type:uma-ticket";

        public static string ticketNo = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c.";
    }
}
