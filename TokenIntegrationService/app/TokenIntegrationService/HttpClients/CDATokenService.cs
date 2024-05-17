using TokenIntegrationService.Models;

namespace TokenIntegrationService.HttpClients
{
    public class CDATokenService : ICDATokenService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public CDATokenService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }        

        public async Task<RptsModel> PostRpt(CDATokenRequestModel request)
        {
            var client = _httpClientFactory.CreateClient("CDAToken");

            client.DefaultRequestHeaders.Add("X-Request-ID", request.RequestId);
            client.BaseAddress = new Uri(request.CdaTokenUrl!);

            var response = await client!.PostAsync(ConstructEndPoint(request), null);                      

            var result = await response.Content.ReadFromJsonAsync<RptsModel>();

            return result!;
        }   
        
        private string ConstructEndPoint (CDATokenRequestModel request)
        {
            return $"token?grant_type={request.GrantType}" +
                $"&ticket={request.Ticket}" +
                $"&claim_token_format={request.ClaimTokenFormat}" +
                $"&claim_token={request.ClaimToken}" +
                $"&scope={request.Scope}";
        }
    }
}
