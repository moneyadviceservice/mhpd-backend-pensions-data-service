using Newtonsoft.Json;
using NUnit.Framework;
using System.Diagnostics.CodeAnalysis;
using CDAPeIsServiceApiTests.Support;
using CDAServiceApiTests.Support;


namespace CDAServiceApiTests.StepDefinitions
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
                UriBuilder uriBuilder = new UriBuilder("http", "localhost", 5089, "token");
                uriBuilder.Query = Parameters.tokenRequestQuery;
                httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, uriBuilder.ToString());
                httpRequestMessage.Headers.Add("X-Request-ID", Parameters.xRequestID);
                httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);
                _scenarioContext["StatusResponse"] = httpResponseMessage.StatusCode.ToString();
                _scenarioContext["ResponseBody"] = httpResponseMessage.Content.ReadAsStringAsync().Result;
            }
            else if (hostedOn.Equals("Azure QA Environment"))
            {
                UriBuilder uriBuilder = new UriBuilder("https", Parameters.azureCdaEmulatorTokenConfigurationsUrl);
                uriBuilder.Query = Parameters.tokenRequestQuery;
                httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, uriBuilder.ToString());
                httpRequestMessage.Headers.Add("X-Request-ID", Parameters.xRequestID);
                httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);
                _scenarioContext["StatusResponse"] = httpResponseMessage.StatusCode.ToString();
                _scenarioContext["ResponseBody"] = httpResponseMessage.Content.ReadAsStringAsync().Result;
            }
        }

        [Then(@"response from token service is all ok with response code as '([^']*)'")]
        public void ThenResponseFromTokenServiceIsAllOkWithResponseCodeAs(string expectedResponseCode)
        {

            var actualStatusResponse = _scenarioContext["StatusResponse"];
            Console.WriteLine($"Expected: {expectedResponseCode}, Actual: {actualStatusResponse}");

            Assert.True(actualStatusResponse.Equals(expectedResponseCode));
        }

        [Then(@"response body contains access_token, token_type, upgraded, pct")]
        public async Task ThenResponseBodyContainsAccess_TokenToken_TypeUpgradedPct()
        {
            var responseContent = await httpResponseMessage!.Content.ReadAsStringAsync();
            var responseData = JsonConvert.DeserializeObject<ResponseDataModel.CdaTokenResponses>(responseContent);
            string? actualResponseAccessToken = responseData.access_token;
            string? actualResponseTokenType = responseData.token_type;
            bool actualResponseUpgraded = responseData.upgraded;
            string? actualResponsePct = responseData.access_token;

            var expectedResponseBody = _scenarioContext["ResponseBody"];

            var jsonExpectedResponseBody = JsonConvert.SerializeObject(expectedResponseBody);
            var expectedResponseDictionary = JsonConvert.DeserializeObject<ResponseDataModel.CdaTokenResponses>(JsonConvert.DeserializeObject<string>(jsonExpectedResponseBody));

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
            bool value = false;
            if (responseBody is not null && expectedValue is not null)
            {
                value = (responseBody.Equals(expectedValue));
            }
            return value;
        }


        [Given(@"user sends post request to '(.*)' with headers as '(.*)' with params as '(.*)' for scope '(.*)' for grant type '(.*)' for ticket '(.*)' for claim token '(.*)' for claim token format")]
        public async Task GivenUserSendsPostRequestToWithHeadersAsWithParamsAsForScopeForGrantTypeForTicketForClaimTokenForClaimTokenFormat
        (string hostedOn, string xRequestId, string scope, string grantType, string ticketNo, string claimToken, string claimTokenFormat)
        {
            if (hostedOn.Equals("localhost"))
            {
                UriBuilder uriBuilder = new UriBuilder("http", "localhost", 5089, "token");
                string query = string.Empty;
                if (scope.Equals(string.Empty))
                    query = "scope=" + "&";
                else if (scope.EndsWith('x'))
                    query = "scope=" + scope + "xxx" + "&";
                else
                    query = "scope=" + scope + "&";
                if (grantType.Equals(string.Empty))
                    query = query + "grant_type=" + "&";
                else if (grantType.EndsWith('x'))
                    query = query + "grant_type=" + Parameters.grantType + "xxx" + "&";
                else
                    query = query + "grant_type=" + Parameters.grantType + "&";
                if (ticketNo.Equals(string.Empty))
                    query = query + "ticket=" + "&";
                else if (ticketNo.EndsWith('x'))
                    query = query + "ticket=" + Parameters.ticketNo + "xxx" + "&";
                else
                    query = query + "ticket=" + Parameters.ticketNo + "&";
                if (claimToken.Equals(string.Empty))
                    query = query + "claim_token=" + "&";
                else if (claimToken.EndsWith('x'))
                    query = query + "claim_token=" + Parameters.ticketNo + "xxx" + "&";
                else
                    query = query + "claim_token=" + Parameters.ticketNo + "&";
                if (claimTokenFormat.Equals(string.Empty))
                    query = query + "claim_token_format=";
                else if (claimTokenFormat.EndsWith('x'))
                    query = query + "claim_token_format=" + claimTokenFormat + "xxx";
                else
                    query = query + "claim_token_format=" + claimTokenFormat;
                uriBuilder.Query = query;
                httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, uriBuilder.ToString());

                if (xRequestId.Equals(string.Empty))
                    httpRequestMessage.Headers.Add("X-Request-ID", string.Empty);
                else if (xRequestId.EndsWith('x'))
                    httpRequestMessage.Headers.Add("X-Request-ID", xRequestId);
                else
                    httpRequestMessage.Headers.Add("X-Request-ID", Parameters.xRequestID);

                httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);
                _scenarioContext["StatusResponse"] = httpResponseMessage.StatusCode.ToString();
                _scenarioContext["ResponseBody"] = httpResponseMessage.Content.ReadAsStringAsync().Result;
            }
            else if (hostedOn.Equals("Azure QA Environment"))
            {
                UriBuilder uriBuilder = new UriBuilder("https", Parameters.azureCdaEmulatorTokenConfigurationsUrl);
                string query = string.Empty;
                if (scope.Equals(string.Empty))
                    query = "scope=" + "&";
                else if (scope.EndsWith('x'))
                    query = "scope=" + scope + "xxx" + "&";
                else
                    query = "scope=" + scope + "&";
                if (grantType.Equals(string.Empty))
                    query = query + "grant_type=" + "&";
                else if (grantType.EndsWith('x'))
                    query = query + "grant_type=" + Parameters.grantType + "xxx" + "&";
                else
                    query = query + "grant_type=" + Parameters.grantType + "&";
                if (ticketNo.Equals(string.Empty))
                    query = query + "ticket=" + "&";
                else if (ticketNo.EndsWith('x'))
                    query = query + "ticket=" + Parameters.ticketNo + "xxx" + "&";
                else
                    query = query + "ticket=" + Parameters.ticketNo + "&";
                if (claimToken.Equals(string.Empty))
                    query = query + "claim_token=" + "&";
                else if (claimToken.EndsWith('x'))
                    query = query + "claim_token=" + Parameters.ticketNo + "xxx" + "&";
                else
                    query = query + "claim_token=" + Parameters.ticketNo + "&";
                if (claimTokenFormat.Equals(string.Empty))
                    query = query + "claim_token_format=";
                else if (claimTokenFormat.EndsWith('x'))
                    query = query + "claim_token_format=" + claimTokenFormat + "xxx";
                else
                    query = query + "claim_token_format=" + claimTokenFormat;
                uriBuilder.Query = query;
                httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, uriBuilder.ToString());

                if (xRequestId.Equals(string.Empty))
                    httpRequestMessage.Headers.Add("X-Request-ID", string.Empty);
                else if (xRequestId.EndsWith('x'))
                    httpRequestMessage.Headers.Add("X-Request-ID", xRequestId);
                else
                    httpRequestMessage.Headers.Add("X-Request-ID", Parameters.xRequestID);

                httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);
                _scenarioContext["StatusResponse"] = httpResponseMessage.StatusCode.ToString();
                _scenarioContext["ResponseBody"] = httpResponseMessage.Content.ReadAsStringAsync().Result;
            }
        }

    }
}
