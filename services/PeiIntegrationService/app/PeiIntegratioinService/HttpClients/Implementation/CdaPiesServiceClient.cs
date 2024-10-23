using System.Net.Http.Headers;
using System.Web;
using MhpdCommon.Constants;
using MhpdCommon.Constants.HttpClient;
using PeiIntegrationService.HttpClients.Interfaces;
using PeiIntegrationService.Models.CdaPeisServiceClient;
using PeiIntegrationService.Models.CdaPiesService;

namespace PeiIntegrationService.HttpClients.Implementation;

public class CdaPiesServiceClient(IHttpClientFactory httpClientFactory) : ICdaPiesServiceClient
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

    public async Task<CdaPiesServiceResponseModel?> GetPiesAsync(CdaPiesServiceRequestModel request)
    {
        var client = _httpClientFactory.CreateClient(HttpClientNames.CdaService);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(HeaderConstants.AuthenticateType, request.Rpt);
        client.DefaultRequestHeaders.Add(HeaderConstants.RequestId, request.RequestId);

        var endPoint = string.Format(HttpEndpoints.External.CdaPeis, HttpUtility.UrlEncode(request.PeisId));

        var response = await client!.GetAsync(endPoint);

        return CreateResponse(response).Result;
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
