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
        HttpRequestMessage buildPeisEndPoint(string hostedOn)
        {
            RequestBodyData requestBodyData = new RequestBodyData();
            requestBodyData.requestId = Parameters.requestBodyRequestId;
            requestBodyData.peisId = Parameters.peisId;
            var data = JsonConvert.SerializeObject(requestBodyData);
            var contentData = new StringContent(data, Encoding.UTF8, "application/json");
            UriBuilder uriBuilder = new UriBuilder();
            if (hostedOn.Equals("localhost"))
            {
                uriBuilder = new UriBuilder("http", Parameters.localHostUri, Parameters.portNo, "peis");
            }
            else if ((hostedOn.Equals("Azure QA Environment")))
            {
                uriBuilder = new UriBuilder("https", Parameters.azureUrl);
            }            
            Uri uri = uriBuilder.Uri;
            var httpRequestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = uri,
                Content = contentData
            };            
            httpRequestMessage.Headers.Add("iss", Parameters.iss);
            httpRequestMessage.Headers.Add("userSessionId", Parameters.userSessionId);
            httpRequestMessage.Headers.Add("rpt", Parameters.AuthorisationCode);
            return httpRequestMessage;
        }

        [StepDefinition(@"user sends get request to '([^']*)' peis endpoint")]
        public async Task GivenUserSendsGetRequestToPeisEndpoint(string hostedOn)
        {
            httpRequestMessage = buildPeisEndPoint(hostedOn);
            httpResponseMessage = await httpClient.SendAsync(httpRequestMessage).ConfigureAwait(false);
            _scenarioContext["StatusResponse"] = httpResponseMessage.StatusCode.ToString();
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
                if (item.pei is not null)
                {
                    string[] peiIds = item.pei.Split(":");
                    Assert.IsTrue(peiIds[0].Length == 36);
                    Assert.IsTrue(peiIds[1].Length == 36);
                }
                if (item.description is not null)
                {
                    Assert.IsTrue(item.description.Equals("Pension Bee"));
                }
                if (item.retrievalStatus is not null)
                {
                    Assert.IsTrue(item.retrievalStatus.Equals("NEW"));
                }
                if (item.retrievalRequestedTimestamp is not null)
                {
                    Assert.IsTrue(item.retrievalRequestedTimestamp.HasValue);
                }
            }
        }
        HttpRequestMessage buildPeisEndPointWithParameters
            (string hostedOn, string iss, string userSessionId, string rpt, string requestId, string peisId)
        {
            RequestBodyData requestBodyData = new RequestBodyData();
            if (!(requestId.Equals(string.Empty)))
            {
                requestBodyData.requestId = requestId;
            }
            if (!(peisId.Equals(string.Empty)))
            {
                requestBodyData.peisId = peisId;
            }
            var data = JsonConvert.SerializeObject(requestBodyData);
            var contentData = new StringContent(data, Encoding.UTF8, "application/json");
            UriBuilder uriBuilder = new UriBuilder();
            if (hostedOn.Equals("localhost"))
            {
                uriBuilder = new UriBuilder("http", Parameters.localHostUri, Parameters.portNo, "peis");
            }
            else if ((hostedOn.Equals("Azure QA Environment")))
            {
                uriBuilder = new UriBuilder("https", Parameters.azureUrl);
            }            
            Uri uri = uriBuilder.Uri;
            var httpRequestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = uri,
                Content = contentData
            };            
            if (!(iss.Equals(string.Empty)))
            {
                httpRequestMessage.Headers.Add("iss", iss);
            }
            if (!(userSessionId.Equals(string.Empty)))
            {
                httpRequestMessage.Headers.Add("userSessionId", userSessionId);
            }
            if (!(rpt.Equals(string.Empty)))
            {
                httpRequestMessage.Headers.Add("rpt", rpt);
            }
            return httpRequestMessage;
        }

        [StepDefinition(@"get request sent to '([^']*)' with headers as '([^']*)' for iss '([^']*)' for sessionid '([^']*)' for authorisation with request body having '([^']*)' for request id '([^']*)' for request peisId")]
        public async Task GivenGetRequestSentToWithHeadersAsForIssForSessionidForAuthorisationWithRequestBodyHavingForRequestIdForRequestPeisId
            (string hostedOn, string issValue, string userSessionIdValue, string rptValue, string requestIdValue, string peisId)
        {
            httpRequestMessage = buildPeisEndPointWithParameters(hostedOn, issValue, userSessionIdValue, rptValue, requestIdValue, peisId);
            httpResponseMessage = await httpClient.SendAsync(httpRequestMessage).ConfigureAwait(false);
            _scenarioContext["StatusResponse"] = httpResponseMessage.StatusCode.ToString();            
        }

        [StepDefinition(@"user sends get request to '([^']*)' peis endpoint with missing iss")]
        public async Task GivenUserSendsGetRequestToPeisEndpointWithMissingIss(string hostedOn)
        {
            httpRequestMessage = buildPeisEndPointWithParameters(hostedOn, string.Empty, Parameters.userSessionId, Parameters.AuthorisationCode, Parameters.requestBodyRequestId, Parameters.peisId);
            httpResponseMessage = await httpClient.SendAsync(httpRequestMessage).ConfigureAwait(false);
            _scenarioContext["StatusResponse"] = httpResponseMessage.StatusCode.ToString();
        }

        [StepDefinition(@"user sends get request to '([^']*)' peis endpoint with missing userSessionId")]
        public async Task GivenUserSendsGetRequestToPeisEndpointWithMissingUserSessionId(string hostedOn)
        {
            httpRequestMessage = buildPeisEndPointWithParameters(hostedOn, Parameters.iss, string.Empty, Parameters.AuthorisationCode, Parameters.requestBodyRequestId, Parameters.peisId);
            httpResponseMessage = await httpClient.SendAsync(httpRequestMessage).ConfigureAwait(false);
            _scenarioContext["StatusResponse"] = httpResponseMessage.StatusCode.ToString();
        }

        [StepDefinition(@"user sends get request to '([^']*)' peis endpoint with missing rpt")]
        public async Task GivenUserSendsGetRequestToPeisEndpointWithMissingRpt(string hostedOn)
        {
            httpRequestMessage = buildPeisEndPointWithParameters(hostedOn, Parameters.iss, Parameters.userSessionId, string.Empty, Parameters.requestBodyRequestId, Parameters.peisId);
            httpResponseMessage = await httpClient.SendAsync(httpRequestMessage).ConfigureAwait(false);
            _scenarioContext["StatusResponse"] = httpResponseMessage.StatusCode.ToString();
        }

        [StepDefinition(@"user sends get request to '([^']*)' peis endpoint with missing requestId")]
        public async Task GivenUserSendsGetRequestToPeisEndpointWithMissingRequestId(string hostedOn)
        {
            httpRequestMessage = buildPeisEndPointWithParameters(hostedOn, Parameters.iss, Parameters.userSessionId, Parameters.AuthorisationCode, string.Empty, Parameters.peisId);
            httpResponseMessage = await httpClient.SendAsync(httpRequestMessage).ConfigureAwait(false);
            _scenarioContext["StatusResponse"] = httpResponseMessage.StatusCode.ToString();
        }

        [StepDefinition(@"user sends get request to '([^']*)' peis endpoint with missing peisId")]
        public async Task GivenUserSendsGetRequestToPeisEndpointWithMissingPeisId(string hostedOn)
        {
            httpRequestMessage = buildPeisEndPointWithParameters(hostedOn, Parameters.iss, Parameters.userSessionId, Parameters.AuthorisationCode, Parameters.requestBodyRequestId, string.Empty);
            httpResponseMessage = await httpClient.SendAsync(httpRequestMessage).ConfigureAwait(false);
            _scenarioContext["StatusResponse"] = httpResponseMessage.StatusCode.ToString();
        }
    }
}

