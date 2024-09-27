using System.Text;
using System.Text.Json;
using PeiIntegrationService.HttpClients.Interfaces;
using PeiIntegrationService.Models;
using PeiIntegrationService.Models.TokenIntegrationService;

namespace PeiIntegrationService.HttpClients.Implementation
{
    public class TokenIntegrationServiceClient : ITokenIntegrationServiceClient
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        private readonly string _cdaTokenServicesEndpoint;
        private readonly string _mapsTokenIntegrationServiceEndpoint;

        public TokenIntegrationServiceClient(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _cdaTokenServicesEndpoint = _configuration[Constants.HttpEndpoints.CdaTokenServicesEndpoint]!;
            _mapsTokenIntegrationServiceEndpoint = _configuration[Constants.HttpEndpoints.TokenIntegrationServiceEndpoint]!;
        }
        public async Task<TokenIntegrationResponseModel> PostRpt(TokenIntegrationServiceRequestModel request)
        {
            var client = _httpClientFactory.CreateClient(Constants.HttpClients.TokenIntegrationService);
            client.BaseAddress = new Uri(_mapsTokenIntegrationServiceEndpoint);

            request.As_Uri = _cdaTokenServicesEndpoint; // <<<======== this should come in through via wwwAuthenticate header as_uri
            var payload = JsonSerializer.Serialize(request);

            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            var responseTokenInt = await client!.PostAsync(Constants.RequestRoutes.Rpt, content);
            var result = responseTokenInt.Content.ReadFromJsonAsync<TokenIntegrationResponseModel>().Result;
            return result!;
        }
    }
}
