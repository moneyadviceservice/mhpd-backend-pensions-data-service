using System;
using System.Text;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.DataCollection;
using Newtonsoft.Json;
using NUnit.Framework;
using TokenIntegrationServiceApiTests.Support;
using static TokenIntegrationServiceApiTests.Support.ResponseDataModel;

namespace TokenIntegrationServiceApiTests.StepDefinitions
{
    [Binding]
    public sealed class TokenIntegrationServiceStepDefinitions
    {
        private HttpClient httpClient;
        public static HttpRequestMessage? httpRequestMessage;
        public static HttpResponseMessage? httpResponseMessage;
        private readonly ScenarioContext _scenarioContext;

        public TokenIntegrationServiceStepDefinitions(ScenarioContext scenarioContext)
        {
            httpClient = new HttpClient();
            _scenarioContext = scenarioContext;
        }        

        [StepDefinition(@"user sends post request to '([^']*)' Token Integration Service endpoint")]
        public async Task GivenUserSendsPostRequestToTokenIntegrationServiceEndpoint(string hostedOn)
        {
            HttpRequestMessage httpRequest = new HttpRequestMessage();
            RequestBodyData requestBodyData = new RequestBodyData();
            if (hostedOn.Equals("localhost"))
                httpRequest = buildHttpRequestMessageForService(requestBodyData, "localhost");
            else if (hostedOn.Equals("Azure QA Environment"))
                httpRequest = buildHttpRequestMessageForService(requestBodyData, Parameters.azureTokenIntegrationServiceUrl);
            httpResponseMessage = await httpClient.SendAsync(httpRequest);
            _scenarioContext["StatusResponse"] = httpResponseMessage.StatusCode.ToString();
            _scenarioContext["ResponseBody"] = httpResponseMessage.Content.ReadAsStringAsync().Result;
        }

        HttpRequestMessage buildHttpRequestMessageForService(RequestBodyData requestBodyData, string hostedOn)
        {
            requestBodyData.ticket = Parameters.ticketNo;
            requestBodyData.rqp = Parameters.rqp;
            requestBodyData.as_uri = Parameters.localHostAsUri;            
            UriBuilder uriBuilder = new UriBuilder();
            if (hostedOn.Equals("localhost"))
            {
                requestBodyData.as_uri = Parameters.localHostAsUri;
                uriBuilder = new UriBuilder("http", "localhost", 5289, "rpt");
            }
            else
            {
                requestBodyData.as_uri = Parameters.azureTokenServiceBaseUrl;
                uriBuilder = new UriBuilder("https", Parameters.azureTokenIntegrationServiceUrl);
            }
            var data = JsonConvert.SerializeObject(requestBodyData);
            var contentData = new StringContent(data, Encoding.UTF8, "application/json");
            Uri uri = uriBuilder.Uri;
            var httpRequestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = uri,
                Content = contentData
            };
            
            return httpRequestMessage;
        }

        [StepDefinition(@"response is all ok with response code as '([^']*)'")]
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

        [StepDefinition(@"response body contains rqp")]
        public async Task ThenResponseBodyContainsRqp()
        {
            var responseContent = await httpResponseMessage!.Content.ReadAsStringAsync();
            var responseData = JsonConvert.DeserializeObject<RptTokenResponses>(responseContent);
            string? actualResponseAccessToken = responseData.rpt;
            var expectedResponseBody = _scenarioContext["ResponseBody"];
            var jsonExpectedResponseBody = JsonConvert.SerializeObject(expectedResponseBody);
            var expectedResponseDictionary = JsonConvert.DeserializeObject<RptTokenResponses>(JsonConvert.DeserializeObject<string>(jsonExpectedResponseBody));
            var expectedAccessToken = expectedResponseDictionary.rpt;
            if (expectedAccessToken is not null)
            {
                bool accessTokenResponseCondition = checkResponseBodyAsExpected(actualResponseAccessToken, expectedAccessToken);
                Assert.True(accessTokenResponseCondition);
            }
        }

        RequestBodyData buildRequestBody(string hostedOn, RequestBodyData requestBodyData, string ticket, string rqp, string as_uri)
        {
            if (ticket.Equals(string.Empty))
            {
                requestBodyData.ticket = ticket;
            }
            else if (ticket.EndsWith('x'))
            {
                requestBodyData.ticket = Parameters.ticketNo + "xxx";
            }
            else
            {
                requestBodyData.ticket = Parameters.ticketNo;
            }
            if (rqp.Equals(string.Empty))
            {
                requestBodyData.rqp = rqp;
            }
            else if (rqp.EndsWith('x'))
            {
                requestBodyData.rqp = Parameters.rqp + "xxx";
            }
            else
            {
                requestBodyData.rqp = Parameters.rqp;
            }

            if (hostedOn.Equals("localhost"))
            {
                if (as_uri.Equals(string.Empty))
                {
                    requestBodyData.as_uri = as_uri;
                }
                else if (as_uri.EndsWith('x'))
                {
                    requestBodyData.as_uri = Parameters.localHostAsUri + "xxx";
                }
                else
                {
                    requestBodyData.as_uri = Parameters.localHostAsUri;
                }
            }
            else
            {
                if (as_uri.Equals(string.Empty))
                {
                    requestBodyData.as_uri = as_uri;
                }
                else if (as_uri.EndsWith('x'))
                {
                    requestBodyData.as_uri = Parameters.azureAsUri + "xxx";
                }
                else
                {
                    requestBodyData.as_uri = Parameters.azureTokenServiceBaseUrl;
                }
            }
            return requestBodyData;
        }


        [StepDefinition(@"user sends post request to '([^']*)' with body as '([^']*)' for rqp '([^']*)' for ticket '([^']*)' for as_uri")]
        public async Task GivenUserSendsPostRequestToWithBodyAsForRqpForTicketForAs_Uri(string hostedOn, string rqp, string ticket, string asUri)
        {
            RequestBodyData requestBodyData = new RequestBodyData();
            if (hostedOn.Equals("localhost"))
                requestBodyData = buildRequestBody(hostedOn, requestBodyData, ticket, rqp, asUri);
            else if (hostedOn.Equals("Azure QA Environment"))
                requestBodyData = buildRequestBody(hostedOn, requestBodyData, ticket, rqp, asUri);                
            var data = JsonConvert.SerializeObject(requestBodyData);
            var contentData = new StringContent(data, Encoding.UTF8, "application/json");
            UriBuilder uriBuilder = new UriBuilder();
            if (hostedOn.Equals("localhost"))
                uriBuilder = new UriBuilder("http", Parameters.localHostTokenIntegrationServiceUrl, Parameters.localHostPortNo, "rpt");
            else if (hostedOn.Equals("Azure QA Environment"))
                uriBuilder = new UriBuilder("https", Parameters.azureTokenIntegrationServiceUrl);
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
