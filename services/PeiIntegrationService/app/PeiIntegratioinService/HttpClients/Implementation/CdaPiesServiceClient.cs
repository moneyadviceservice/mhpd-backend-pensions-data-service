using System.Net.Http.Headers;
using System.Web;
using MhpdCommon.Constants;
using MhpdCommon.Constants.HttpClient;
using MhpdCommon.Extensions;
using PeiIntegrationService.HttpClients.Interfaces;
using PeiIntegrationService.Models.CdaPeisServiceClient;
using PeiIntegrationService.Models.CdaPiesService;

namespace PeiIntegrationService.HttpClients.Implementation;

public class CdaPiesServiceClient(IHttpClientFactory httpClientFactory, ILogger<CdaPiesServiceClient> logger) : ICdaPiesServiceClient
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly ILogger<CdaPiesServiceClient> _logger = logger;

    public async Task<CdaPiesServiceResponseModel?> GetPiesAsync(CdaPiesServiceRequestModel request)
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

    private static async Task<CdaPiesServiceResponseModel> CreateResponse(HttpResponseMessage? response)
    {
        if (response!.StatusCode == System.Net.HttpStatusCode.OK)
        {
            var result = await response!.Content.ReadFromJsonAsync<CdaPeiApiResponse>();

            ApplyRetrievalStatus(result!);

            return new CdaPiesServiceResponseModel
            {
                Peis = [.. result!.PeiList!],
                ResponseMessage = new ResponseMessage
                {
                    ResponseStatusCode = System.Net.HttpStatusCode.OK
                }
            };
        }

        return new CdaPiesServiceResponseModel
        {
            Peis = null,
            ResponseMessage = new ResponseMessage
            {
                ResponseStatusCode = response!.StatusCode,
                WWWAuthenticateResponseHeader = response.Headers.WwwAuthenticate.ToString()
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
