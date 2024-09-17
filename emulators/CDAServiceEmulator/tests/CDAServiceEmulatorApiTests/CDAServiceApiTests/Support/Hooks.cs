using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CDAPeIsServiceApiTests.Support
{
    public class Hooks : BaseClass
    {
        [BeforeScenario]
        public static void SetUpUserForScenario()
        {
            
        }
        [AfterScenario]
        public static void DeleteUser()
        {
            httpClient?.Dispose();
        }
    }
}
