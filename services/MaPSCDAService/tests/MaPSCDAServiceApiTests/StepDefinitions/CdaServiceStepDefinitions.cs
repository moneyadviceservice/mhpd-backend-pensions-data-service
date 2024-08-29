using System.Text;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.DataCollection;
using Newtonsoft.Json;
using NUnit.Framework;
using MaPSCDAServiceApiTests.Support;
using static System.Formats.Asn1.AsnWriter;
using static MaPSCDAServiceApiTests.Support.ResponseDataModel;

namespace TokenIntegrationServiceApiTests.StepDefinitions
{
    [Binding]
    public sealed class CdaServiceStepDefinitions
    {
        private HttpClient httpClient;
        public static HttpRequestMessage? httpRequestMessage;
        public static HttpResponseMessage? httpResponseMessage;
        private readonly ScenarioContext _scenarioContext;

        public CdaServiceStepDefinitions(ScenarioContext scenarioContext)
        {
            httpClient = new HttpClient();
            _scenarioContext = scenarioContext;
        }       

        [StepDefinition(@"user sends post request to '([^']*)' CDA Service endpoint")]
        public async Task GivenUserSendsPostRequestToCDAServiceEndpoint(string hostedOn)
        {           
            if (hostedOn.Equals("localhost"))
            {
                RequestBodyData requestBodyData = new RequestBodyData();
                requestBodyData = buildRequestBody(requestBodyData, Parameters.iss, Parameters.userSessionId);
                var data = JsonConvert.SerializeObject(requestBodyData);
                var contentData = new StringContent(data, Encoding.UTF8, "application/json");

                UriBuilder uriBuilder = new UriBuilder("http", "localhost", 5047, "rqp");
                Uri uri = uriBuilder.Uri;
                var httpRequestMessage = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = uri,
                    Content = contentData
                };
                
                httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);
                _scenarioContext["StatusResponse"] = httpResponseMessage.StatusCode.ToString();
                _scenarioContext["ResponseBody"] = httpResponseMessage.Content.ReadAsStringAsync().Result;
            }
            else if (hostedOn.Equals("Azure QA Environment"))
            {
                RequestBodyData requestBodyData = new RequestBodyData();
                requestBodyData = buildRequestBody(requestBodyData, Parameters.iss, Parameters.userSessionId);
                var data = JsonConvert.SerializeObject(requestBodyData);
                var contentData = new StringContent(data, Encoding.UTF8, "application/json");

                UriBuilder uriBuilder = new UriBuilder("https", Parameters.azureCdaServiceUrl);
                Uri uri = uriBuilder.Uri;
                var httpRequestMessage = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = uri,
                    Content = contentData
                };

                httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);
                _scenarioContext["StatusResponse"] = httpResponseMessage.StatusCode.ToString();
                _scenarioContext["ResponseBody"] = httpResponseMessage.Content.ReadAsStringAsync().Result;                
            }
        }


        [Then(@"response is all ok with response code as '([^']*)'")]
        public void ThenResponseIsAllOkWithResponseCodeAs(string expectedResponseCode)
        {
            var actualStatusResponse = _scenarioContext["StatusResponse"];
            Assert.True(actualStatusResponse.Equals(expectedResponseCode));
        }

        
        bool checkResponseBodyAsExpected(string? responseBody, string expectedValue)
        {
            bool value = false;
            if (responseBody is not null && expectedValue is not null)
            {
                value = (responseBody.Equals(expectedValue));
            }
            return value;
        }

        [Then(@"response body contains rqp")]
        public async Task ThenResponseBodyContainsRqp()
        {
            var responseContent = await httpResponseMessage!.Content.ReadAsStringAsync();
            var responseData = JsonConvert.DeserializeObject<CdaTokenResponses>(responseContent);
            string? actualResponseAccessToken = responseData.rqp;

            var expectedResponseBody = _scenarioContext["ResponseBody"];

            var jsonExpectedResponseBody = JsonConvert.SerializeObject(expectedResponseBody);
            var expectedResponseDictionary = JsonConvert.DeserializeObject<CdaTokenResponses>(JsonConvert.DeserializeObject<string>(jsonExpectedResponseBody));

            var expectedRqpToken = expectedResponseDictionary.rqp;
            if (expectedRqpToken is not null)
            {
                bool rqpResponseCondition = checkResponseBodyAsExpected(actualResponseAccessToken, expectedRqpToken);
                Assert.True(rqpResponseCondition);
            }
        }

        RequestBodyData buildRequestBody(RequestBodyData requestBodyData, string iss, string userSessionId)
        {            
            if (iss.Equals(string.Empty))
            {
                requestBodyData.iss = iss;
            }
            else if (iss.EndsWith('x'))
            {
                requestBodyData.iss = Parameters.iss + "xxx";
            }
            else
            {
                requestBodyData.iss = Parameters.iss;
            }
            if (userSessionId.Equals(string.Empty))
            {
                requestBodyData.userSessionId = userSessionId;
            }
            else if (userSessionId.EndsWith('x'))
            {
                requestBodyData.userSessionId = Parameters.userSessionId + "xxx";
            }
            else
            {
                requestBodyData.userSessionId = Parameters.userSessionId;
            }
            

            return requestBodyData;
        }

        [StepDefinition(@"user sends post request to '([^']*)' with body as '([^']*)' for iss '([^']*)' for userSessionId")]
        public async Task GivenUserSendsPostRequestToWithBodyAsForIssForUserSessionId(string hostedOn, string iss, string userSessionId)
        {            
            if (hostedOn.Equals("localhost"))
            {
                RequestBodyData requestBodyData = new RequestBodyData();
                requestBodyData = buildRequestBody(requestBodyData, iss, userSessionId);
                var data = JsonConvert.SerializeObject(requestBodyData);
                var contentData = new StringContent(data, Encoding.UTF8, "application/json");

                UriBuilder uriBuilder = new UriBuilder("http", Parameters.localHostCdaServiceUrl, Parameters.localHostPortNo, "rqp");

                Uri uri = uriBuilder.Uri;
                var httpRequestMessage = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = uri,
                    Content = contentData
                };

                httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);
                _scenarioContext["StatusResponse"] = httpResponseMessage.StatusCode.ToString();
                _scenarioContext["ResponseBody"] = httpResponseMessage.Content.ReadAsStringAsync().Result;
            }
            else if (hostedOn.Equals("Azure QA Environment"))
            {
                RequestBodyData requestBodyData = new RequestBodyData();
                requestBodyData = buildRequestBody(requestBodyData, iss, userSessionId);
                var data = JsonConvert.SerializeObject(requestBodyData);
                var contentData = new StringContent(data, Encoding.UTF8, "application/json");

                UriBuilder uriBuilder = new UriBuilder("https", Parameters.azureCdaServiceUrl);

                Uri uri = uriBuilder.Uri;
                var httpRequestMessage = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = uri,
                    Content = contentData
                };

                httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);
                _scenarioContext["StatusResponse"] = httpResponseMessage.StatusCode.ToString();
                _scenarioContext["ResponseBody"] = httpResponseMessage.Content.ReadAsStringAsync().Result;
            }
        }
    }
}
