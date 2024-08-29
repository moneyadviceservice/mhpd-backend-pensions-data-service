using CDAPeIsServiceApiTests.Support;
using CDAServiceApiTests.Support;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Diagnostics.CodeAnalysis;
using static CDAServiceApiTests.Support.ResponseDataModel;

namespace CDAServiceApiTests.StepDefinitions
{
    [Binding]
    [ExcludeFromCodeCoverage]
    public sealed class CdaPeisStepDefinition
    {
        private HttpClient httpClient;
        public static HttpRequestMessage? httpRequestMessage;
        public static HttpResponseMessage? httpResponseMessage;
        private readonly ScenarioContext _scenarioContext;
        private QueryRequest? queryRequest;

        public CdaPeisStepDefinition(ScenarioContext scenarioContext)
        {
            httpClient = new HttpClient();
            _scenarioContext = scenarioContext;
        }

        [StepDefinition(@"user sends GET request to '([^']*)' endpoint for holder name configurations of cda service emulator")]
        public async Task GivenUserSendsGETRequestToEndpointForHolderNameConfigurationsOfCdaServiceEmulator(string hostedOn)
        {
            httpRequestMessage = buildCdaEmulatorHolderConfigurationsEndpoint(hostedOn);
            httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);
            _scenarioContext["StatusResponse"] = httpResponseMessage.StatusCode.ToString();
        }

        HttpRequestMessage buildCdaEmulatorHolderConfigurationsEndpoint(string hostedOn)
        {
            UriBuilder uriBuilder;
            if (hostedOn.Equals("localhost"))
            {
                uriBuilder = new UriBuilder("http", "localhost", 5089, "holdername-configurations");
            }
            else
            {
                uriBuilder = new UriBuilder("https", Parameters.azureCdaEmulatorHolderNameConfigurationsUrl);
            }            
            httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, uriBuilder.ToString());
            httpRequestMessage.Headers.Add("X-Request-ID", Parameters.guid);                        
            return httpRequestMessage;
        }

        HttpRequestMessage buildCdaEmulatorHolderConfigurationsEndpoint(string hostedOn, string xRequestId)
        {
            UriBuilder uriBuilder;
            if (hostedOn.Equals("localhost"))
            {
                uriBuilder = new UriBuilder("http", "localhost", 5089, "holdername-configurations");
            }
            else
            {
                uriBuilder = new UriBuilder("https", Parameters.azureCdaEmulatorHolderNameConfigurationsUrl);
            }
            httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, uriBuilder.ToString());
            if(xRequestId != "x") 
                httpRequestMessage.Headers.Add("X-Request-ID", xRequestId);            
            return httpRequestMessage;
        }

        HttpRequestMessage buildCdaPeiEndpoint(string hostedOn, string authorisationCondition)
        {
            UriBuilder uriBuilder;
            if (hostedOn.Equals("localhost"))
            {
                uriBuilder = new UriBuilder("http", "localhost", 5089, "peis/0d9b46c0-00fd-4f18-86b2-dfa0994c9ff3");
            }
            else
            {
                uriBuilder = new UriBuilder("https", Parameters.azureCdaEmulatorPeiUrl);
            }
            //uriBuilder.Query = "scope=uma_protection";
            httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, uriBuilder.ToString());
            httpRequestMessage.Headers.Add("X-Request-ID", Parameters.xRequestID);            
            //httpRequestMessage.Headers.Add("X-Version", Parameters.xVersion);
            if (authorisationCondition.Equals("with")) {
                //httpRequestMessage.Headers.Add("Authorisation", Parameters.AuthorisationCode);
                httpRequestMessage.Headers.Add("Authorization", $"Bearer {Parameters.AuthorisationCode}");
            }
            return httpRequestMessage;
        }

        HttpRequestMessage buildCdaPeiEndpointWithoutPeisId(string hostedOn, string authorisationCondition)
        {
            UriBuilder uriBuilder;
            if (hostedOn.Equals("localhost"))
            {
                uriBuilder = new UriBuilder("http", "localhost", 5089, "peis/");
            }
            else
            {
                uriBuilder = new UriBuilder("https", Parameters.azureCdaEmulatorPeiUrlWithoutPeisId);
            }            
            httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, uriBuilder.ToString());
            httpRequestMessage.Headers.Add("X-Request-ID", Parameters.xRequestID);            
            if (authorisationCondition.Equals("with"))
            {                
                httpRequestMessage.Headers.Add("Authorization", $"Bearer {Parameters.AuthorisationCode}");
            }
            return httpRequestMessage;
        }

        [StepDefinition(@"user sends request to '([^']*)' endpoint '([^']*)' RPT authorization")]
        public async Task GivenUserSendsRequestToEndpointRPTAuthorization(string hostedOn, string authorisationCondition)
        {
            httpRequestMessage = buildCdaPeiEndpoint(hostedOn, authorisationCondition);
            httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);
            _scenarioContext["StatusResponse"] = httpResponseMessage.StatusCode.ToString();
        }

        [StepDefinition(@"user sends request to '([^']*)' endpoint '([^']*)' RPT authorization but missing XRequestId")]
        public async Task GivenUserSendsRequestToEndpointRPTAuthorizationButMissingXRequestId(string hostedOn, string authorisationCondition)
        {
            httpRequestMessage = buildCdaPeiEndpoint(hostedOn, authorisationCondition);            
            httpRequestMessage.Headers.Remove("X-Request-ID");
            httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);
            _scenarioContext["StatusResponse"] = httpResponseMessage.StatusCode.ToString();
        }

        [StepDefinition(@"user sends request to '([^']*)' endpoint '([^']*)' RPT authorization but missing PeisId")]
        public async Task GivenUserSendsRequestToEndpointRPTAuthorizationButMissingPeisId(string hostedOn, string authorisationCondition)
        {
            httpRequestMessage = buildCdaPeiEndpointWithoutPeisId(hostedOn, authorisationCondition);            
            httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);
            _scenarioContext["StatusResponse"] = httpResponseMessage.StatusCode.ToString();
        }

        [StepDefinition(@"response is all ok with response code as '([^']*)'")]
        public void ThenResponseIsAllOkWithResponseCodeAs(string expectedResponseCode)
        {
            var actualStatusResponse = _scenarioContext["StatusResponse"];
            Assert.True(actualStatusResponse.Equals(expectedResponseCode));
        }

        [StepDefinition(@"response body contains holder_configurations with expected values")]
        public async Task ThenResponseBodyContainsHolder_ConfigurationsWithExpectedValues()
        {            
            var responseContent = await httpResponseMessage!.Content.ReadAsStringAsync();
            bool result = responseContent.Contains("holder_configurations");
            Assert.True(result);
            var data = JsonConvert.DeserializeObject<HolderConfigurationsList>(responseContent);
            data.holder_configurations.Should().HaveCount(2);
            var holderConfigurationList = data.holder_configurations;
            if (holderConfigurationList is not null)
            {
                var firstElement = holderConfigurationList.ElementAt(0);
                if (firstElement is not null)
                {
                    if(firstElement.holdername_guid is not null)
                        Assert.IsTrue(firstElement.holdername_guid.ToString().Equals(Parameters.firstElementGuid));
                    if (firstElement.veiw_data_url is not null)
                        Assert.IsTrue(firstElement.veiw_data_url.ToString().Equals(Parameters.firstElementViewDataUrl));
                }
                var secondElement = holderConfigurationList.ElementAt(1);
                if (secondElement is not null)
                {
                    if (secondElement.holdername_guid is not null)
                        Assert.IsTrue(secondElement.holdername_guid.ToString().Equals(Parameters.secondElementGuid));
                    if (secondElement.veiw_data_url is not null)
                        Assert.IsTrue(secondElement.veiw_data_url.ToString().Equals(Parameters.secondElementViewDataUrl));
                }
            }
        }

        QueryRequest buildUrl(string hostedOn, string expectedPeisid, string expectedRequestId)
        {
            string? pathValue = null;
            string? queryScope = string.Empty;
            string? baseUrl = string.Empty;
            UriBuilder uriBuilder = new UriBuilder();
            queryRequest = new QueryRequest(uriBuilder);
            if (expectedPeisid.Equals(string.Empty))
            {
                pathValue = "peis/" + expectedPeisid;
                baseUrl = Parameters.cdaServiceEmulatorBaseUrl + pathValue;
            }
            else
            {
                pathValue = "/peis/" + expectedPeisid;
                baseUrl = Parameters.cdaServiceEmulatorBaseUrl + pathValue;
            }            
            if (!(expectedRequestId.Equals(null)))
                queryRequest.xRequestID = expectedRequestId;            
            else
                queryScope = "";
            if (hostedOn.Equals("localhost"))
                queryRequest.uriBuilder = new UriBuilder("http", "localhost", 5089, pathValue);
            else
                queryRequest.uriBuilder = new UriBuilder("https", baseUrl);
            queryRequest.uriBuilder.Query = queryScope;
            return queryRequest;
        }

        [Given(@"user sends request to '([^']*)' endpoint '([^']*)' RPT authorization with request as '([^']*)' with peisid as '([^']*)'")]
        public async Task GivenUserSendsRequestToEndpointRPTAuthorizationWithRequestAsWithPeisidAs(string hostedOn, string authorisationCondition, string expectedRequestId, string expectedPeisid)
        {           
            queryRequest = buildUrl(hostedOn, expectedPeisid, expectedRequestId);
            httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, queryRequest.uriBuilder.ToString());
            httpRequestMessage.Headers.Add("X-Request-ID", queryRequest.xRequestID);
            //httpRequestMessage.Headers.Add("X-Version", queryRequest.xVersion);
            if (authorisationCondition.Equals("with"))
            {
                //httpRequestMessage.Headers.Add("Authorisation", Parameters.AuthorisationCode);
                httpRequestMessage.Headers.Add("Authorization", $"Bearer {Parameters.AuthorisationCode}");
            }
            httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);
            _scenarioContext["StatusResponse"] = httpResponseMessage.StatusCode.ToString();
        }

        [StepDefinition(@"user sends Get request to '([^']*)' endpoint as per '([^']*)'")]
        public async Task GivenUserSendsGetRequestToEndpointAsPer(string hostedOn, string xRequestId)
        {
            httpRequestMessage = buildCdaEmulatorHolderConfigurationsEndpoint(hostedOn, xRequestId);
            httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);
            _scenarioContext["StatusResponse"] = httpResponseMessage.StatusCode.ToString();
        }

        [StepDefinition(@"user sends Get request to '([^']*)' endpoint with missing X-Request-ID")]
        public async Task GivenUserSendsGetRequestToEndpointWithMissingX_Request_ID(string hostedOn)
        {
            httpRequestMessage = buildCdaEmulatorHolderConfigurationsEndpoint(hostedOn,"x");
            httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);
            _scenarioContext["StatusResponse"] = httpResponseMessage.StatusCode.ToString();
        }

    }
}
