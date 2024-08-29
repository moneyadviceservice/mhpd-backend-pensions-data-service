using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.DataCollection;
using Newtonsoft.Json;
using NUnit.Framework;
using PDPViewDataServiceEmulatorApiTests.Support;
using static System.Formats.Asn1.AsnWriter;
using static PDPViewDataServiceEmulatorApiTests.Support.ResponseData;

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

        HttpRequestMessage buildRequest(string hostedOn, string? scopeValue, string? assetGuid)
        {
            UriBuilder uriBuilder = new UriBuilder();
            if (hostedOn.Equals("localhost"))
            {
                uriBuilder = new UriBuilder("http", "localhost", 3000, "token");
            }
            else if (hostedOn.Equals("Azure QA Environment"))
            {
                uriBuilder = new UriBuilder("https", Parameters.azurePDPViewDataServiceEmulatorUrl);
                if (assetGuid != null)
                {
                    uriBuilder.Path = assetGuid;
                }
            }
            if (scopeValue != null)
            {
                uriBuilder.Query = "scope=" + scopeValue;
            }
            httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, uriBuilder.ToString());
            return httpRequestMessage;
        }

        [StepDefinition(@"user sends get request to '([^']*)' pdp view data service endpoint with valid inputs")]
        public async Task GivenUserSendsGetRequestToPdpViewDataServiceEndpointWithValidInputs(string hostedOn)
        {
            httpRequestMessage = buildRequest(hostedOn, Parameters.ownerScope, Parameters.assetGuid);
            httpRequestMessage.Headers.Add("Authorization", $"Bearer {Parameters.authorisationToken}");            
            httpRequestMessage.Headers.Add("X-Request-ID", Parameters.xRequestId);            
            httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);
            _scenarioContext["StatusResponse"] = httpResponseMessage.StatusCode.ToString();
            _scenarioContext["ResponseBody"] = httpResponseMessage.Content.ReadAsStringAsync().Result;
        }

        [StepDefinition(@"user sends post request to '([^']*)' pdp view data service endpoint to '([^']*)' with '([^']*)' as params'([^']*)' as headers '([^']*)' as bearer token")]
        public async Task GivenUserSendsPostRequestToPdpViewDataServiceEndpointToWithAsParamsAsHeadersAsBearerToken(string hostedOn, string xRequestID, string scope, string assetGuid, string authorization)
        {            
            httpRequestMessage = buildRequest(hostedOn, scope, assetGuid);
            httpRequestMessage.Headers.Add("Authorization", $"Bearer {authorization}");
            httpRequestMessage.Headers.Add("X-Request-ID", xRequestID);
            httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);
            _scenarioContext["StatusResponse"] = httpResponseMessage.StatusCode.ToString();
            _scenarioContext["ResponseBody"] = httpResponseMessage.Content.ReadAsStringAsync().Result;
        }

        [StepDefinition(@"user sends get request to '([^']*)' pdp view data service endpoint with missing asset guid")]
        public async Task GivenUserSendsGetRequestToPdpViewDataServiceEndpointWithMissingAssetGuid(string hostedOn)
        {
            httpRequestMessage = buildRequest(hostedOn, Parameters.ownerScope,null);
            httpRequestMessage.Headers.Add("Authorization", $"Bearer {Parameters.authorisationToken}");
            httpRequestMessage.Headers.Add("X-Request-ID", Parameters.xRequestId);
            httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);
            _scenarioContext["StatusResponse"] = httpResponseMessage.StatusCode.ToString();
            _scenarioContext["ResponseBody"] = httpResponseMessage.Content.ReadAsStringAsync().Result;
        }

        [StepDefinition(@"user sends get request to '([^']*)' pdp view data service endpoint with missing scope")]
        public async Task GivenUserSendsGetRequestToPdpViewDataServiceEndpointWithMissingScope(string hostedOn)
        {
            httpRequestMessage = buildRequest(hostedOn, null, Parameters.assetGuid);
            httpRequestMessage.Headers.Add("Authorization", $"Bearer {Parameters.authorisationToken}");
            httpRequestMessage.Headers.Add("X-Request-ID", Parameters.xRequestId);
            httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);
            _scenarioContext["StatusResponse"] = httpResponseMessage.StatusCode.ToString();
            _scenarioContext["ResponseBody"] = httpResponseMessage.Content.ReadAsStringAsync().Result;
        }

        [StepDefinition(@"user sends get request to '([^']*)' pdp view data service endpoint with missing x-request-id")]
        public async Task GivenUserSendsGetRequestToPdpViewDataServiceEndpointWithMissingX_Request_Id(string hostedOn)
        {
            httpRequestMessage = buildRequest(hostedOn, Parameters.ownerScope, Parameters.assetGuid);
            httpRequestMessage.Headers.Add("Authorization", $"Bearer {Parameters.authorisationToken}");
            //httpRequestMessage.Headers.Add("X-Request-ID", Parameters.xRequestId);
            httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);
            _scenarioContext["StatusResponse"] = httpResponseMessage.StatusCode.ToString();
            _scenarioContext["ResponseBody"] = httpResponseMessage.Content.ReadAsStringAsync().Result;
        }

        [StepDefinition(@"user sends get request to '([^']*)' pdp view data service endpoint with missing Authorization")]
        public async Task GivenUserSendsGetRequestToPdpViewDataServiceEndpointWithMissingAuthorization(string hostedOn)
        {
            httpRequestMessage = buildRequest(hostedOn, Parameters.ownerScope, Parameters.assetGuid);
            //httpRequestMessage.Headers.Add("Authorization", $"Bearer {Parameters.authorisationToken}");
            httpRequestMessage.Headers.Add("X-Request-ID", Parameters.xRequestId);
            httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);
            _scenarioContext["StatusResponse"] = httpResponseMessage.StatusCode.ToString();
            _scenarioContext["ResponseBody"] = httpResponseMessage.Content.ReadAsStringAsync().Result;
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

        [StepDefinition(@"response is all ok with response code as '([^']*)'")]
        public void ThenResponseIsAllOkWithResponseCodeAs(string expectedResponseCode)
        {
            var actualStatusResponse = _scenarioContext["StatusResponse"];
            Assert.True(actualStatusResponse.Equals(expectedResponseCode));
        }


        [StepDefinition(@"response header contains value for WWW-Authenticate")]
        public void ThenResponseHeaderContainsValueForWWW_Authenticate()
        {
            HttpResponseHeaders responseHeader = httpResponseMessage!.Headers;
            var actualResponseHeader = responseHeader.GetValues("WWW-Authenticate");
            if (actualResponseHeader.Equals(Parameters.responseHeaderAuthenticate))
                Assert.IsTrue(true);
        }

        [StepDefinition(@"response body contains view_data_token")]
        public async Task ThenResponseBodyContainsView_Data_Token()
        {
            var responseContent = await httpResponseMessage!.Content.ReadAsStringAsync();
            var responseData = JsonConvert.DeserializeObject<ViewDataResponse>(responseContent);
            string? actualResponseViewDataToken = responseData.view_data_token;
            Assert.IsTrue(actualResponseViewDataToken is not null);            
        }

    }
}
