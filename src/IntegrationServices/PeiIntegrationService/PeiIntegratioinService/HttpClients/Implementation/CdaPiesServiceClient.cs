using System.Net.Http.Headers;
using PeiIntegrationService.HttpClients.Interfaces;
using PeiIntegrationService.Models.CdaPiesService;

namespace PeiIntegrationService.HttpClients.Implementation
{
    public class CdaPiesServiceClient : ICdaPiesServiceClient
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _cdaPeisServiceEndpoint;

        public CdaPiesServiceClient(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _cdaPeisServiceEndpoint = _configuration["CdaPeisServiceEndpoint"]!;
        }

        public async Task<CdaPiesServiceResponseModel?> GetPiesAsync(CdaPiesServiceRequestModel request)
        {
            var client = _httpClientFactory.CreateClient("CdaPiesService");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", request.Rpt);
            client.DefaultRequestHeaders.Add("X-Request-ID", request.RequestId);
            client.BaseAddress = new Uri(_cdaPeisServiceEndpoint!);

            var endPoint = $"{request.PeisId}";

            var response = await client!.GetAsync(endPoint);

            return CreateResponse(response).Result;
        }

        private async Task<CdaPiesServiceResponseModel> CreateResponse(HttpResponseMessage? response)
        {
            if (response!.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var result = await response!.Content.ReadFromJsonAsync<PeiModel[]>();

                ApplyRetrievalStatus(ref result!);

                return new CdaPiesServiceResponseModel
                {
                    Peis = result,
                    ResponseMessage = new ResponseMessage
                    {
                        ResponseStatusCode = "200"
                    }
                };
            }

            return new CdaPiesServiceResponseModel
            {
                Peis = null,
                ResponseMessage = new ResponseMessage
                {
                    ResponseStatusCode = response!.StatusCode.ToString(),
                    WWWAuthenticateResponseHeader = response.Headers.WwwAuthenticate.ToString()
                }
            };

        }

        private void ApplyRetrievalStatus(ref PeiModel[] resultPei)
        {
            foreach (var pei in resultPei)
            {
                pei.RetrievalStatus = RetrievelStatusEnum.NEW;
                pei.RetrievalRequestedTimestamp = DateTime.UtcNow;
            }
        }
    }
}
