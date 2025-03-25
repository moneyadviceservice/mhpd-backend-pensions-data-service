using System.Net;
using System.Net.Http.Headers;
using System.Web;
using MhpdCommon.Constants;
using MhpdCommon.Constants.HttpClient;
using MhpdCommon.Extensions;
using MhpdCommon.Models.MHPDModels;
using Microsoft.Extensions.Logging;
using PensionRequestFunction.Models.CdaPeisServiceClient;

namespace PensionRequestFunction.HttpClient;

public  class PdpViewDataClient(IHttpClientFactory httpClientFactory, ILogger<PdpViewDataClient> logger) : IPdpViewDataClient
{
    public async Task<PdpServiceResponseModel> GetPdpViewDataAsync(string assetGuid, string viewDataUrl, string? rpt, string correlationId)       
    {
        const string scope = "owner";
        var client = httpClientFactory.CreateClient(HttpClientNames.PdpService);

        client.DefaultRequestHeaders.Add(HeaderConstants.RequestId, Guid.NewGuid().ToString());
        client.DefaultRequestHeaders.Add(HeaderConstants.CorrelationId, correlationId);
        client.DefaultRequestHeaders.Add(HeaderConstants.ProviderUrl, viewDataUrl);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(HeaderConstants.AuthenticateType, rpt);

        var endPoint = string.Format(HttpEndpoints.External.PdpViewData, HttpUtility.UrlEncode(assetGuid), scope);

        var viewDataResponse = await client.GetAsync(endPoint);
        
        var response = await CreateResponse(viewDataResponse);

        logger.LogResponse(response);

        return response;
    }

    private static async Task<PdpServiceResponseModel> CreateResponse(HttpResponseMessage? response)
    {
        if (response!.StatusCode == HttpStatusCode.OK)
        {
            var result = await response.Content.ReadAsStringAsync();

            return new PdpServiceResponseModel
            {
                ViewDataToken = result,
                ResponseMessage = new ResponseMessage
                {
                    ResponseStatusCode = HttpStatusCode.OK
                }
            };
        }

        return new PdpServiceResponseModel
        {
            ViewDataToken = null,
            ResponseMessage = new ResponseMessage
            {
                ResponseStatusCode = response.StatusCode,
                WwwAuthenticateResponseHeader = response.Headers.WwwAuthenticate.ToString()
            }
        };
    }
}