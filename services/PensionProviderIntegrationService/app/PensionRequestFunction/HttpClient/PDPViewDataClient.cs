using System.Net.Http.Headers;
using MhpdCommon.Constants;
using MhpdCommon.Extensions;
using Microsoft.Extensions.Logging;
using PensionRequestFunction.Models.CdaPeisServiceClient;

namespace PensionRequestFunction.HttpClient;

public  class PdpViewDataClient(IHttpClientFactory httpClientFactory, ILogger<PdpViewDataClient> logger) : IPdpViewDataClient
{
    public async Task<PdpServiceResponseModel> GetPdpViewDataAsync(string assetGuid, string viewDataUrl, string? rpt, string correlationId)       
    {
        var scope = "owner";
        var client = httpClientFactory.CreateClient();

        client.DefaultRequestHeaders.Add(HeaderConstants.RequestId, Guid.NewGuid().ToString());
        client.DefaultRequestHeaders.Add(HeaderConstants.CorrelationId, correlationId);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(HeaderConstants.AuthenticateType, rpt);

        var viewDataResponse = await client.GetAsync($"{viewDataUrl}/{assetGuid}?scope={scope}");
        
        var response = await CreateResponse(viewDataResponse);

        logger.LogResponse(response);

        return response;
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