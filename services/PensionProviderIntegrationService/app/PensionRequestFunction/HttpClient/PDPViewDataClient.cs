using System.Net.Http.Headers;
using PensionRequestFunction.Models.CdaPeisServiceClient;

namespace PensionRequestFunction.HttpClient
{
    public  class PDPViewDataClient : IPDPViewDataClient
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public PDPViewDataClient(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<PDPServiceResponseModel> GetPDPViewDataAsync(string assetGuid, string viewDataUrl, string rpt)       
        {
            var scope = "owner";
            var client = _httpClientFactory.CreateClient("PDPViewData");

            client.DefaultRequestHeaders.Add("X-Request-ID", Guid.NewGuid().ToString());
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", rpt);

            var response = await client.GetAsync($"{viewDataUrl}{assetGuid}?scope={scope}");
            
            return CreateResponse(response).Result;
        }

        private async Task<PDPServiceResponseModel> CreateResponse(HttpResponseMessage? response)
        {
            if (response!.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var result = await response!.Content.ReadAsStringAsync();


                return new PDPServiceResponseModel
                {
                    ViewDataToken = result,
                    ResponseMessage = new ResponseMessage
                    {
                        ResponseStatusCode = "200"
                    }
                };
            }

            return new PDPServiceResponseModel
            {
                ViewDataToken = null,
                ResponseMessage = new ResponseMessage
                {
                    ResponseStatusCode = response!.StatusCode.ToString(),
                    WWWAuthenticateResponseHeader = response.Headers.WwwAuthenticate.ToString()
                }
            };

        }
    }
}