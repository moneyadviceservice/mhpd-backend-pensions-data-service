using System.Net.Http.Headers;
using System.Web;
using MhpdCommon.Constants;
using MhpdCommon.Constants.HttpClient;
using MhpdCommon.Extensions;
using MhpdCommon.Models.MHPDModels;
using PeiIntegrationService.HttpClients.Interfaces;
using PeiIntegrationService.Models.CdaPeisServiceClient;
using PeiIntegrationService.Models.CdaPiesService;

namespace PeiIntegrationService.HttpClients.Implementation;

public class CdaPeisServiceClient(IHttpClientFactory httpClientFactory, ILogger<CdaPeisServiceClient> logger) : ICdaPiesServiceClient
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly ILogger<CdaPeisServiceClient> _logger = logger;

    public async Task<CdaPeisServiceResponseModel?> GetPiesAsync(CdaPiesServiceRequestModel request)
    {
        _logger.LogRequest(request);

        var client = _httpClientFactory.CreateClient(HttpClientNames.CdaService);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(HeaderConstants.AuthenticateType, request.Rpt);
        client.DefaultRequestHeaders.Add(HeaderConstants.RequestId, Guid.NewGuid().ToString());
        client.DefaultRequestHeaders.Add(HeaderConstants.CorrelationId, request.CorrelationId);

        var endPoint = string.Format(HttpEndpoints.External.CdaPeis, HttpUtility.UrlEncode(request.PeisId));

        var clientResponse = await client!.GetAsync(endPoint);

        var response = CreateResponse(clientResponse).Result;

        _logger.LogResponse(response);
        return response;
    }

    private static async Task<CdaPeisServiceResponseModel> CreateResponse(HttpResponseMessage? response)
    {
        if (response!.StatusCode == System.Net.HttpStatusCode.OK)
        {
            var result = await response!.Content.ReadFromJsonAsync<CdaPeiApiResponse>();

            ApplyRetrievalStatus(result!);

            return new CdaPeisServiceResponseModel
            {
                Peis = [.. result!.PeiList!],
                ResponseMessage = new ResponseMessage
                {
                    ResponseStatusCode = System.Net.HttpStatusCode.OK
                }
            };
        }

        return new CdaPeisServiceResponseModel
        {
            Peis = null,
            ResponseMessage = new ResponseMessage
            {
                ResponseStatusCode = response!.StatusCode,
                WwwAuthenticateResponseHeader = response.Headers.WwwAuthenticate.ToString()
            }
        };

    }

    private static void ApplyRetrievalStatus(CdaPeiApiResponse response)
    {
        if (response.PeiList == null) return;

        foreach (var pei in response.PeiList)
        {
            pei.RetrievalStatus = RetrievelStatusEnum.NEW;
            pei.RetrievalRequestedTimestamp = DateTime.UtcNow;
        }
    }
}
