using Newtonsoft.Json;
using NUnit.Framework;
using PeiIntegrationServiceApiTests.Support;
using System.Net.Http.Headers;
using System.Text;
using static PeiIntegrationServiceApiTests.Support.PeiResponseDataModel;

namespace PeiIntegrationServiceApiTests.StepDefinitions
{
    [Binding]
    public sealed class PeiIntegrationStepDefinition
    {
        private HttpClient httpClient;
        public static HttpRequestMessage? httpRequestMessage;
        public static HttpResponseMessage? httpResponseMessage;
        private readonly ScenarioContext _scenarioContext;

        public PeiIntegrationStepDefinition(ScenarioContext scenarioContext)
        {
            httpClient = new HttpClient();
            _scenarioContext = scenarioContext;
        }

        [StepDefinition(@"user sends get request to '([^']*)' peis endpoint")]
        public async Task GivenUserSendsGetRequestToPeisEndpoint(string hostedOn)
        {
            if (hostedOn.Equals("localhost"))
            {
                RequestBodyData requestBodyData = new RequestBodyData();
                requestBodyData.requestId = Parameters.requestBodyRequestId;
                requestBodyData.peisBaseUrl = Parameters.localHostRequestBodyPeisBaseUrl;
                var data = JsonConvert.SerializeObject(requestBodyData);
                var contentData = new StringContent(data, Encoding.UTF8, "application/json");

                UriBuilder uriBuilder = new UriBuilder("http", Parameters.localHostUri, Parameters.portNo, "peis");
                uriBuilder.Query = "scope=owner";
                Uri uri = uriBuilder.Uri;
                var httpRequestMessage = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = uri,
                    Content = contentData
                };
                httpRequestMessage.Headers.Add("cdaUserGuid", Parameters.guid);
                httpRequestMessage.Headers.Add("iss", Parameters.iss);
                httpRequestMessage.Headers.Add("userSessionId", Parameters.userSessionId);
                httpRequestMessage.Headers.Add("rpt", Parameters.AuthorisationCode);
                httpResponseMessage = await httpClient.SendAsync(httpRequestMessage).ConfigureAwait(false);
                _scenarioContext["StatusResponse"] = httpResponseMessage.StatusCode.ToString();
            }
            else if (hostedOn.Equals("Azure QA Environment"))
            {
                RequestBodyData requestBodyData = new RequestBodyData();
                requestBodyData.requestId = Parameters.requestBodyRequestId;
                requestBodyData.peisBaseUrl = Parameters.azureHostRequestBodyPeisBaseUrl;
                var data = JsonConvert.SerializeObject(requestBodyData);
                var contentData = new StringContent(data, Encoding.UTF8, "application/json");
                UriBuilder uriBuilder = new UriBuilder("https", Parameters.azureUrl);
                uriBuilder.Query = "scope=owner";
                Uri uri = uriBuilder.Uri;
                var httpRequestMessage = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = uri,
                    Content = contentData
                };
                httpRequestMessage.Headers.Add("cdaUserGuid", Parameters.guid);
                httpRequestMessage.Headers.Add("iss", Parameters.iss);
                httpRequestMessage.Headers.Add("userSessionId", Parameters.userSessionId);
                httpRequestMessage.Headers.Add("rpt", Parameters.AuthorisationCode);
                httpResponseMessage = await httpClient.SendAsync(httpRequestMessage).ConfigureAwait(false);
                _scenarioContext["StatusResponse"] = httpResponseMessage.StatusCode.ToString();
            }
        }

        [StepDefinition(@"response is all ok with response code as '([^']*)'")]
        public void ThenResponseIsAllOkWithResponseCodeAs(string expectedResponseCode)
        {
            var actualStatusResponse = _scenarioContext["StatusResponse"];
            Assert.True(actualStatusResponse.Equals(expectedResponseCode));
        }

        [StepDefinition(@"response header contains rpt")]
        public void ThenResponseHeaderContainsRpt()
        {
            HttpResponseHeaders responseHeader = httpResponseMessage!.Headers;
            var actualRptValue = responseHeader.GetValues("rpt");
            foreach (var value in actualRptValue)
            {
                if (value.Equals(Parameters.AuthorisationCode))
                {
                    Assert.IsTrue(true);
                    break;
                }
            }
        }

        [StepDefinition(@"response body contains pei with description, retrievalStatus, retrievalRequestedTimestamp")]
        public async Task ThenResponseBodyContainsPeiWithDescriptionRetrievalStatusRetrievalRequestedTimestamp()
        {
            var responseContent = await httpResponseMessage!.Content.ReadAsStringAsync();
            var responseData = JsonConvert.DeserializeObject<List<PeiResponses>>(responseContent);
            foreach (var item in responseData)
            {
                string[] peiIds = item.pei.Split(":");
                Assert.IsTrue(peiIds[0].Length == 36);
                Assert.IsTrue(peiIds[1].Length == 36);
                Assert.IsTrue(item.description.Equals("Pension Bee"));
                Assert.IsTrue(item.retrievalStatus.Equals("NEW"));
                Assert.IsTrue(item.retrievalRequestedTimestamp.HasValue);
            }
        }

        [Given(@"get request sent to '([^']*)' with headers as '([^']*)' for guid '([^']*)' for iss '([^']*)' for sessionid '([^']*)' for authorisation with params as '([^']*)' request body having '([^']*)' for request id '([^']*)' for request url")]
        public async Task GivenGetRequestSentToWithHeadersAsForGuidForIssForSessionidForAuthorisationWithParamsAsRequestBodyHavingForRequestIdForRequestUrl
            (string hostedOn, string cdaUserGuidValue, string issValue, string userSessionIdValue, string rptValue, string scope, string requestIdValue, string peiBaseUrlValue)
        {
            string? queryScope = string.Empty;
            string? baseUrl = string.Empty;

            if (hostedOn.Equals("localhost"))
            {
                RequestBodyData requestBodyData = new RequestBodyData();
                if (!(requestIdValue.Equals(string.Empty)))
                    requestBodyData.requestId = requestIdValue;
                if (peiBaseUrlValue.Equals(string.Empty))
                    requestBodyData.peisBaseUrl = string.Empty;
                else if (peiBaseUrlValue.EndsWith('x'))
                    requestBodyData.peisBaseUrl = Parameters.localHostRequestBodyPeisBaseUrl + "xxx";
                else
                    requestBodyData.peisBaseUrl = Parameters.localHostRequestBodyPeisBaseUrl;
                var data = JsonConvert.SerializeObject(requestBodyData);
                var contentData = new StringContent(data, Encoding.UTF8, "application/json");
                UriBuilder uriBuilder = new UriBuilder("http", Parameters.localHostUri, Parameters.portNo, "peis");
                if (scope.Equals(string.Empty))
                    uriBuilder.Query = "scope=";
                else if (scope.Equals("owner"))
                    uriBuilder.Query = "scope=owner";
                else
                {
                    var queryBuilder = "scope" + scope;
                    uriBuilder.Query = queryBuilder;
                }

                Uri uri = uriBuilder.Uri;
                var httpRequestMessage = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = uri,
                    Content = contentData
                };

                if (!(cdaUserGuidValue.Equals(string.Empty)))
                    httpRequestMessage.Headers.Add("cdaUserGuid", cdaUserGuidValue);
                if (!(issValue.Equals(string.Empty)))
                    httpRequestMessage.Headers.Add("iss", issValue);
                if (!(userSessionIdValue.Equals(string.Empty)))
                    httpRequestMessage.Headers.Add("userSessionId", userSessionIdValue);
                if (!(rptValue.Equals(string.Empty)))
                    httpRequestMessage.Headers.Add("rpt", rptValue);
                httpResponseMessage = await httpClient.SendAsync(httpRequestMessage).ConfigureAwait(false);
                _scenarioContext["StatusResponse"] = httpResponseMessage.StatusCode.ToString();
            }

            else if (hostedOn.Equals("Azure QA Environment"))
            {
                RequestBodyData requestBodyData = new RequestBodyData();
                if (!(requestIdValue.Equals(string.Empty)))
                    requestBodyData.requestId = requestIdValue;
                if (!(peiBaseUrlValue.Equals(string.Empty)))
                    requestBodyData.peisBaseUrl = peiBaseUrlValue;
                var data = JsonConvert.SerializeObject(requestBodyData);
                var contentData = new StringContent(data, Encoding.UTF8, "application/json");
                UriBuilder uriBuilder = new UriBuilder("https", Parameters.azureUrl);
                if (scope.Equals(string.Empty))
                    uriBuilder.Query = "scope=";
                else if (scope.Equals("owner"))
                    uriBuilder.Query = "scope=owner";
                else
                {
                    var queryBuilder = "scope" + scope;
                    uriBuilder.Query = queryBuilder;
                }

                Uri uri = uriBuilder.Uri;
                var httpRequestMessage = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = uri,
                    Content = contentData
                };

                if (!(cdaUserGuidValue.Equals(string.Empty)))
                    httpRequestMessage.Headers.Add("cdaUserGuid", cdaUserGuidValue);
                if (!(issValue.Equals(string.Empty)))
                    httpRequestMessage.Headers.Add("iss", issValue);
                if (!(userSessionIdValue.Equals(string.Empty)))
                    httpRequestMessage.Headers.Add("userSessionId", userSessionIdValue);
                if (!(rptValue.Equals(string.Empty)))
                    httpRequestMessage.Headers.Add("rpt", rptValue);
                httpResponseMessage = await httpClient.SendAsync(httpRequestMessage).ConfigureAwait(false);
                _scenarioContext["StatusResponse"] = httpResponseMessage.StatusCode.ToString();
            }
        }
    }
}
