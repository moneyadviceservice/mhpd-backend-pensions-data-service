using System.Net;
using System.Net.Http.Headers;
using System.Web;
using MhpdCommon.Constants;
using MhpdCommon.Constants.HttpClient;
using MhpdCommon.Extensions;
using MhpdCommon.Models.MHPDModels;
using MhpdCommon.SharedHttpClient;
using MhpdCommon.Utils;
using Microsoft.Extensions.Logging;
using PensionRequestFunction.Models.CdaPeisServiceClient;

namespace PensionRequestFunction.HttpClient;

public class PdpViewDataClient(IHttpClientFactory httpClientFactory, ILogger<PdpViewDataClient> logger) 
    : BaseHttpClientExecutor(httpClientFactory, logger), IPdpViewDataClient
{
    public async Task<PdpServiceResponseModel> GetPdpViewDataAsync(string assetGuid, string viewDataUrl, string? rpt, string correlationId)       
    {
        const string scope = "owner";
        var assetPath = string.Format(HttpEndpoints.External.PdpViewData, HttpUtility.UrlEncode(assetGuid), scope);
        var providerUrl = UrlHelper.ConstructPath(viewDataUrl, assetPath);

        logger.LogInformation("Get ViewData called for providerUrl {ProviderUrl}", providerUrl);

        var viewDataResponse = await ExecuteAsync(HttpClientNames.PdpService,
            HttpClientOperationName.PdpViewDataGet,
            _ => new HttpRequestMessage(HttpMethod.Get, assetPath),
            message =>
            {
                message.Headers.Add(HeaderConstants.RequestId, Guid.NewGuid().ToString());
                message.Headers.Add(HeaderConstants.CorrelationId, correlationId);
                message.Headers.Add(HeaderConstants.ProviderUrl, providerUrl);

                if (!string.IsNullOrWhiteSpace(rpt))
                {
                    message.Headers.Authorization = new AuthenticationHeaderValue(HeaderConstants.AuthenticateType, rpt);
                }
            });

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
                WwwAuthenticateResponseHeader = GetWwwAuthenticateHeader(response)
            }
        };
    }
    
    private static string? GetWwwAuthenticateHeader(HttpResponseMessage response)
    {
        // Look for any header containing 'www-authenticate' (case-insensitive)
        var matchingHeader = response.Headers
            .FirstOrDefault(h => h.Key.Contains(HeaderConstants.WwwAuthenticate, StringComparison.OrdinalIgnoreCase));

        return matchingHeader.Value != null ? string.Join(" ", matchingHeader.Value) : null;
    }
}