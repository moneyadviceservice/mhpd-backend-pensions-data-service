using System.Text;
using System.Text.Json;
using MhpdCommon.Constants;
using MhpdCommon.Constants.HttpClient;
using MhpdCommon.Models.Configuration;
using MhpdCommon.Utils;
using Microsoft.Extensions.Options;
using PeiIntegrationService.HttpClients.Interfaces;
using PeiIntegrationService.Models;
using PeiIntegrationService.Models.TokenIntegrationService;

namespace PeiIntegrationService.HttpClients.Implementation;

public class TokenIntegrationServiceClient(IHttpClientFactory httpClientFactory, IOptions<CommonHttpConfiguration> configuration) : ITokenIntegrationServiceClient
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly string _cdaTokenServicesEndpoint = UrlHelper.ConstructPath(configuration.Value.CdaServiceUrl, HttpEndpoints.External.CdaTokenServiceEndpoint);

    public async Task<TokenIntegrationResponseModel> PostRpt(TokenIntegrationServiceRequestModel request)
    {
        var client = _httpClientFactory.CreateClient(HttpClientNames.TokenIntegrationService);
        
        // Add request ID header
        // Note: this should be passed in from the entrypoint ot the backend services
        client.DefaultRequestHeaders.Add(HeaderConstants.RequestId, Guid.NewGuid().ToString());

        request.As_Uri = _cdaTokenServicesEndpoint; // <<<======== this should come in through via wwwAuthenticate header as_uri
        var payload = JsonSerializer.Serialize(request);

        var content = new StringContent(payload, Encoding.UTF8, "application/json");
        var responseTokenInt = await client.PostAsync(HttpEndpoints.Internal.Rpts, content);
        var result = responseTokenInt.Content.ReadFromJsonAsync<TokenIntegrationResponseModel>().Result;
        return result!;
    }
}
