using System.Net.Http.Headers;
using PeiIntegrationService.HttpClients.Interfaces;
using PeiIntegrationService.Models;
using PeiIntegrationService.Models.CdaPeisServiceClient;
using PeiIntegrationService.Models.CdaPiesService;

namespace PeiIntegrationService.HttpClients.Implementation;

public class CdaPiesServiceClient : ICdaPiesServiceClient
{
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _cdaPeisServiceEndpoint;

    public CdaPiesServiceClient(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _cdaPeisServiceEndpoint = _configuration[Constants.HttpEndpoints.CdaPeisServiceEndpoint]!;
    }

    public async Task<CdaPiesServiceResponseModel?> GetPiesAsync(CdaPiesServiceRequestModel request)
    {
        var client = _httpClientFactory.CreateClient(Constants.HttpClients.CdaPiesService);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(Constants.Headers.AuthenticateType, request.Rpt);
        client.DefaultRequestHeaders.Add(Constants.Headers.RequestId, request.RequestId);
        client.BaseAddress = new Uri(_cdaPeisServiceEndpoint!);

        var endPoint = $"{request.PeisId}";

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
