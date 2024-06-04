using System.Net.Http;
using System.Net.Http.Headers;
using NUnit.Framework;
using PDPViewDataServiceEmulatorApiTests.Support;

namespace PDPViewDataServiceEmulatorApiTests.StepDefinitions
{
    [Binding]
    public sealed class PDPViewDataServiceEmulatorStepDefinitions
    {
        private HttpClient httpClient;
        public static HttpRequestMessage? httpRequestMessage;
        public static HttpResponseMessage? httpResponseMessage;
        private readonly ScenarioContext _scenarioContext;

        public PDPViewDataServiceEmulatorStepDefinitions(ScenarioContext scenarioContext)
        {
            httpClient = new HttpClient();
            _scenarioContext = scenarioContext;
        }

        HttpRequestMessage buildRequest(string hostedOn)
        {
            UriBuilder uriBuilder = new UriBuilder();
            if (hostedOn.Equals("localhost"))
            {
                uriBuilder = new UriBuilder("http", "localhost", 3000, "token");
            }
            else if (hostedOn.Equals("Azure QA Environment"))
            {
                uriBuilder = new UriBuilder("https", Parameters.azurePDPViewDataServiceEmulatorUrl);
            }
            httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, uriBuilder.ToString());
            return httpRequestMessage;
        }

        [StepDefinition(@"user sends get request to '([^']*)' pdp view data service endpoint")]
        public async Task GivenUserSendsGetRequestToPdpViewDataServiceEndpoint(string hostedOn)
        {
            httpRequestMessage = buildRequest(hostedOn);
            httpRequestMessage.Headers.Add("Authorisation", string.Empty);
            httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);
            _scenarioContext["StatusResponse"] = httpResponseMessage.StatusCode.ToString();
            _scenarioContext["ResponseBody"] = httpResponseMessage.Content.ReadAsStringAsync().Result;
        }

        [StepDefinition(@"user sends get request to '([^']*)' pdp view data service endpoint with missing authorisation header")]
        public async Task GivenUserSendsGetRequestToPdpViewDataServiceEndpointWithMissingAuthorisationHeader(string hostedOn)
        {
            httpRequestMessage = buildRequest(hostedOn);
            httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);
            _scenarioContext["StatusResponse"] = httpResponseMessage.StatusCode.ToString();
            _scenarioContext["ResponseBody"] = httpResponseMessage.Content.ReadAsStringAsync().Result;
        }


        [StepDefinition(@"response is Unauthorized")]
        public void ThenResponseIsUnauthorized()
        {
            var actualStatusResponse = _scenarioContext["StatusResponse"];
            Assert.True(actualStatusResponse.Equals("Unauthorized"));
        }

        [StepDefinition(@"response header contains value for WWW-Authenticate")]
        public void ThenResponseHeaderContainsValueForWWW_Authenticate()
        {
            HttpResponseHeaders responseHeader = httpResponseMessage!.Headers;
            var actualResponseHeader = responseHeader.GetValues("WWW-Authenticate");
            if (actualResponseHeader.Equals(Parameters.responseHeaderAuthenticate))
                Assert.IsTrue(true);
        }
    }
}
