using MhpdCommon.Constants;
using MhpdCommon.Constants.HttpClient;
using MhpdCommon.Models.Configuration;
using MhpdCommon.Utils;
using Microsoft.Extensions.Options;
using PensionRequestFunction.HttpClient.Interfaces;
using PensionRequestFunction.Models.TokenIntegrationServiceClient;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace PensionRequestFunction.HttpClient.Implementation;

public class TokenIntegrationServiceClient(IHttpClientFactory httpClientFactory, IOptions<CommonHttpConfiguration> configuration) : ITokenIntegrationServiceClient
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly CommonHttpConfiguration httpConfiguration = configuration.Value;

    public async Task<TokenIntegrationResponseModel> PostRptAsync(TokenIntegrationServiceRequestModel request)
    {
        var client = _httpClientFactory.CreateClient(HttpClientNames.TokenIntegrationService);
        client.DefaultRequestHeaders.Add(HeaderConstants.RequestId, Guid.NewGuid().ToString());

        request.As_Uri = UrlHelper.ConstructPath(httpConfiguration.CdaServiceUrl, HttpEndpoints.External.CdaTokenServiceEndpoint);
        var payload = JsonSerializer.Serialize(request); 
        var content = new StringContent(payload, Encoding.UTF8, "application/json");
        var responseTokenInt = await client!.PostAsync(HttpEndpoints.Internal.Rpts, content);
        var result = responseTokenInt.Content.ReadFromJsonAsync<TokenIntegrationResponseModel>().Result;
        return result!;
    }
}
