using CDATokenServicesApiTests.Support;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using Newtonsoft.Json;
using NUnit.Framework;
using SpecFlow.Internal.Json;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using TechTalk.SpecFlow.CommonModels;
using static CDATokenServicesApiTests.Support.ResponseDataModel;

namespace CDATokenServicesApiTests.StepDefinitions
{
    [Binding]
    [ExcludeFromCodeCoverage]
    public sealed class CdaTokenServiceStepDefinitions
    {
        private HttpClient httpClient;
        public static HttpRequestMessage? httpRequestMessage;
        public static HttpResponseMessage? httpResponseMessage;
        private readonly ScenarioContext _scenarioContext;

        public CdaTokenServiceStepDefinitions(ScenarioContext scenarioContext)
        {
            httpClient = new HttpClient();
            _scenarioContext = scenarioContext;
        }

        [StepDefinition(@"user sends post request to '([^']*)' cda token service endpoint")]
        public async Task GivenUserSendsPostRequestToCdaTokenServiceEndpoint(string hostedOn)
        {
            if (hostedOn.Equals("localhost"))
            {
                UriBuilder uriBuilder = new UriBuilder("http", "localhost", 5044, "token");
                uriBuilder.Query = "grant_type=urn:ietf:params:oauth:grant-type:jwt-bearer&ticket=eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c.&claim_token_format=pension_dashboad_rqp";
                httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, uriBuilder.ToString());
                httpRequestMessage.Headers.Add("X-Request-ID", Parameters.xRequestID);
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

        [Then(@"response body contains access_token, token_type, upgraded, pct")]
        public async Task ThenResponseBodyContainsAccess_TokenToken_TypeUpgradedPct()
        {
            var responseContent = await httpResponseMessage!.Content.ReadAsStringAsync();
            var responseData = JsonConvert.DeserializeObject<CdaTokenResponses>(responseContent);
            string? actualResponseAccessToken = responseData.access_token;
            string? actualResponseTokenType = responseData.token_type;
            bool actualResponseUpgraded = responseData.upgraded;
            string? actualResponsePct = responseData.access_token;

            var expectedResponseBody = _scenarioContext["ResponseBody"];

            var jsonExpectedResponseBody = JsonConvert.SerializeObject(expectedResponseBody);
            var expectedResponseDictionary = JsonConvert.DeserializeObject<CdaTokenResponses>(JsonConvert.DeserializeObject<string>(jsonExpectedResponseBody));
            
            var expectedAccessToken = expectedResponseDictionary.access_token;
            if (expectedAccessToken is not null)
            {
                bool accessTokenResponseCondition = checkResponseBodyAsExpected(actualResponseAccessToken, expectedAccessToken);
                Assert.True(accessTokenResponseCondition);
            }
            
            var expectedTokenType = expectedResponseDictionary.token_type;
            if (expectedTokenType is not null)
            {
                bool tokenTypeResponseCondition = checkResponseBodyAsExpected(actualResponseTokenType, expectedTokenType);
                Assert.True(tokenTypeResponseCondition);
            }
            
            var expectedUpgraded = expectedResponseDictionary.upgraded;
            Assert.AreEqual(actualResponseUpgraded, expectedUpgraded);

            var expectedPct = expectedResponseDictionary.pct;
            if (expectedPct is not null)
            {
                bool pctCondition = checkResponseBodyAsExpected(actualResponsePct, expectedPct);
                Assert.True(pctCondition);
            }            
        }

        bool checkResponseBodyAsExpected(string? responseBody, string expectedValue)
        {
            bool value=false;                        
            if (responseBody is not null && expectedValue is not null)
            {
                value= (responseBody.Equals(expectedValue));
            }
            return value;
        }

        [StepDefinition(@"user sends post request to '([^']*)'with headers as '([^']*)' with params as '([^']*)' for grant type '([^']*)' for ticket '([^']*)' for claim token format")]
        public async Task GivenUserSendsPostRequestToWithHeadersAsWithParamsAsForGrantTypeForTicketForClaimTokenFormat(string hostedOn, string xRequestId, string grantType, string ticketNo, string claimTokenFormat)
        {
            if (hostedOn.Equals("localhost"))
            {
                UriBuilder uriBuilder = new UriBuilder("http", "localhost", 5044, "token");
                string query=string.Empty;
                if (!(grantType.Equals(string.Empty)))
                    query = "grant_type=" + grantType + "&";
                if (!(ticketNo.Equals(string.Empty)))
                    query = query+"ticket=" + ticketNo + "&";
                if (!(claimTokenFormat.Equals(string.Empty)))
                    query = query+"claim_token_format=" + claimTokenFormat;
                uriBuilder.Query = query;
                httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, uriBuilder.ToString());
                if (!(xRequestId.Equals(string.Empty)))
                    httpRequestMessage.Headers.Add("X-Request-ID", Parameters.xRequestID);
                httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);
                _scenarioContext["StatusResponse"] = httpResponseMessage.StatusCode.ToString();
                _scenarioContext["ResponseBody"] = httpResponseMessage.Content.ReadAsStringAsync().Result;
            }
        }

    }
}
