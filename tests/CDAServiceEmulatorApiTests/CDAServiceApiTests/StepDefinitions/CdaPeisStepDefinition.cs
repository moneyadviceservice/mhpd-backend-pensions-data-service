using CDAPeIsServiceApiTests.Support;
using Microsoft.AspNetCore.Authentication;
using NUnit.Framework;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace CDAServiceApiTests.StepDefinitions
{
    [Binding]
    public sealed class CdaPeisStepDefinition
    {
        private HttpClient httpClient;
        public static HttpRequestMessage? httpRequestMessage;
        public static HttpResponseMessage? httpResponseMessage;
        private readonly ScenarioContext _scenarioContext;

        public CdaPeisStepDefinition(ScenarioContext scenarioContext)
        {
            httpClient = new HttpClient();
            _scenarioContext = scenarioContext;
        }


        [StepDefinition(@"user sends request to '([^']*)' endpoint '([^']*)' RPT authorization")]
        public async Task GivenUserSendsRequestToEndpointRPTAuthorization(string hostedOn, string authorisationCondition)
        {
            if (hostedOn.Equals("localhost"))
            {
                UriBuilder uriBuilder = new UriBuilder("http", "localhost", 5089, "peis/0d9b46c0-00fd-4f18-86b2-dfa0994c9ff3");
                uriBuilder.Query = "scope=uma_protection";
                httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, uriBuilder.ToString());
                httpRequestMessage.Headers.Add("X-Request-ID", Parameters.xRequestID);
                httpRequestMessage.Headers.Add("X-Version", Parameters.xVersion);
                if (authorisationCondition.Equals("with"))
                    httpRequestMessage.Headers.Add("Authorisation", Parameters.AuthorisationCode);
                httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);
                _scenarioContext["StatusResponse"] = httpResponseMessage.StatusCode.ToString();
            }

            else if (hostedOn.Equals("Azure QA Environment"))
            {
                UriBuilder uriBuilder = new UriBuilder("https", Parameters.azureUrl);
                uriBuilder.Query = "scope=uma_protection";
                httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, uriBuilder.ToString());
                httpRequestMessage.Headers.Add("X-Request-ID", Parameters.xRequestID);
                httpRequestMessage.Headers.Add("X-Version", Parameters.xVersion);
                if (authorisationCondition.Equals("with"))
                    httpRequestMessage.Headers.Add("Authorisation", Parameters.AuthorisationCode);
                httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);
                _scenarioContext["StatusResponse"] = httpResponseMessage.StatusCode.ToString();
            }
        }

        [StepDefinition(@"response is all ok with response code as '([^']*)'")]
        public void ThenResponseIsAllOkWithResponseCodeAs(string expectedResponseCode)
        {
            var actualStatusResponse = _scenarioContext["StatusResponse"];
            Assert.True(actualStatusResponse.Equals(expectedResponseCode));
        }

        [StepDefinition(@"user sends request to '([^']*)' endpoint '([^']*)' RPT authorization as per '([^']*)' with request as '([^']*)' and version as '([^']*)' with guid as '([^']*)'")]
        public async Task GivenUserSendsRequestToEndpointRPTAuthorizationAsPerOwnerWithRequestAsAndVersionAsWithGuidAs
            (string hostedOn, string authorisationCondition, string expectedScope, string expectedRequestId, string expectedVersion, string expectedGuid)
        {
            string? pathValue = null;
            string? xRequestID = null;
            string? xVersion = null;
            string? queryScope = string.Empty;
            string? baseUrl = string.Empty;
            UriBuilder uriBuilder = new UriBuilder();
            if (hostedOn.Equals("localhost"))
            {
                if (expectedGuid.Equals(string.Empty))
                {
                    pathValue = "peis/" + expectedGuid;
                    baseUrl = Parameters.azureBaseUrl;
                }
                else
                {
                    pathValue = "/peis/" + expectedGuid;
                    baseUrl = Parameters.azureBaseUrl + pathValue;
                }
                if (!(expectedVersion.Equals(null)))
                    xVersion = expectedVersion;
                if (!(expectedRequestId.Equals(null)))
                    xRequestID = expectedRequestId;
                if (!(expectedScope.Equals(string.Empty)))
                    queryScope = "scope=" + expectedScope;
                else
                    queryScope = "";

                uriBuilder = new UriBuilder("http", "localhost", 5089, pathValue);
            }
            else if (hostedOn.Equals("Azure QA Environment"))
            {
                if (expectedGuid.Equals(string.Empty))
                {
                    pathValue = "peis/" + expectedGuid;
                    baseUrl = Parameters.azureBaseUrl;
                }
                else
                {
                    pathValue = "/peis/" + expectedGuid;
                    baseUrl = Parameters.azureBaseUrl + pathValue;
                }
                if (!(expectedVersion.Equals(null)))
                    xVersion = expectedVersion;
                if (!(expectedRequestId.Equals(null)))
                    xRequestID = expectedRequestId;
                if (!(expectedScope.Equals(string.Empty)))
                    queryScope = "scope=" + expectedScope;
                else
                    queryScope = "";                
                uriBuilder = new UriBuilder("https", baseUrl);
            }

            uriBuilder.Query = queryScope;
            httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, uriBuilder.ToString());
            httpRequestMessage.Headers.Add("X-Request-ID", xRequestID);
            httpRequestMessage.Headers.Add("X-Version", xVersion);
            if (authorisationCondition.Equals("with"))
                httpRequestMessage.Headers.Add("Authorisation", Parameters.AuthorisationCode);
            httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);
            _scenarioContext["StatusResponse"] = httpResponseMessage.StatusCode.ToString();
        }
    }
}
