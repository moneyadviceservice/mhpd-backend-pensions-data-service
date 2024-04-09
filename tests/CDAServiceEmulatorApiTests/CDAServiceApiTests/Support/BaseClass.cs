using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDAPeIsServiceApiTests.Support
{
    public class BaseClass
    {
        public static HttpClient? httpClient;
        public static HttpRequestMessage? httpRequestMessage;
        public static HttpResponseMessage? httpResponseMessage;        

        public BaseClass()
        {
            httpClient = new HttpClient();            
        }
    }
}
