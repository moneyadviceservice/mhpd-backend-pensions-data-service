using System.Net.Http.Headers;
using MhpdCommon.Constants;
using PensionRequestFunction.Models.CdaPeisServiceClient;

namespace PensionRequestFunction.HttpClient;

public  class PdpViewDataClient(IHttpClientFactory httpClientFactory) : IPdpViewDataClient
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

    public async Task<PdpServiceResponseModel> GetPdpViewDataAsync(string assetGuid, string viewDataUrl, string? rpt)       
    {
        var scope = "owner";
        var client = _httpClientFactory.CreateClient();

        client.DefaultRequestHeaders.Add(HeaderConstants.RequestId, Guid.NewGuid().ToString());
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(HeaderConstants.AuthenticateType, rpt);

        var response = await client.GetAsync($"{viewDataUrl}/{assetGuid}?scope={scope}");
        
        return CreateResponse(response).Result;
    }

    private static async Task<PdpServiceResponseModel> CreateResponse(HttpResponseMessage? response)
    {
        if (response!.StatusCode == System.Net.HttpStatusCode.OK)
        {
            var result = await response!.Content.ReadAsStringAsync();

            return new PdpServiceResponseModel
            {
                ViewDataToken = result,
                ResponseMessage = new ResponseMessage
                {
                    ResponseStatusCode = "200"
                }
            };
        }

        return new PdpServiceResponseModel
        {
            ViewDataToken = null,
            ResponseMessage = new ResponseMessage
            {
                ResponseStatusCode = response.StatusCode.ToString(),
                WWWAuthenticateResponseHeader = response.Headers.WwwAuthenticate.ToString()
            }
        };
    }
}