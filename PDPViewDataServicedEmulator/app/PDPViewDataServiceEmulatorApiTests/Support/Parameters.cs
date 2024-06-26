using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDPViewDataServiceEmulatorApiTests.Support
{
    internal class Parameters
    {
        public static string responseHeaderAuthenticate = "realm=\"PensionDashboard\", as_uri=\"https://auth-server/\", ticket=\"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.cThIIoDvwdueQB468K5xDc5633seEFoqwxjF_xSJyQQ\"";

        public static string azurePDPViewDataServiceEmulatorUrl = "pdpviewdataservicedemulator.azurewebsites.net/view-data";

        public static string assetGuid = "1ba03e25-659a-43b8-ae77-b956df168969";

        public static string azurePDPViewDataServiceEmulatorUrlWithassetGuid = azurePDPViewDataServiceEmulatorUrl + assetGuid;

        public static string authorisationToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJsb2dnZWRJbkFzIjoiYWRtaW4iLCJpYXQiOjE0MjI3Nzk2Mzh9.gzSraSYS8EXBxLN_oWnFSRgCzcmJmMjLiuyu5CSpyHI";

        public static string xRequestId = "b7301d11-f166-499a-9bf1-0598c2f1af52";

        public static string ownerScope = "owner";

        
    }
}
