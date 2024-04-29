using TokenIntegrationService.Models;
namespace TokenIntegrationService.HttpClients
{
    public class CDAToken : ICDAToken
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public CDAToken(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        public async Task<RptsModel[]?> PostRpts(CDATokenRequestModel request)
        {
            var client = _httpClientFactory.CreateClient("CDAToken");
            client.DefaultRequestHeaders.Add("X-Request-ID", request.RequestId);
            client.BaseAddress = new Uri(request.CdaTokenUrl!);          
            var endPoint = $"token?grant_type={request.GrantType}&ticket={request.Ticket}&claim_token_format={request.ClaimTokenFormat}&claim_token={request.ClaimToken}&scope={request.Scope}";
            var response = await client!.PostAsync(endPoint,null);
            
            var result = await response.Content.ReadFromJsonAsync<RptsModel[]>();
            ApplyRetrievalStatus(ref result!);
            return result;

        }
        private void ApplyRetrievalStatus(ref RptsModel[] result)
        {
            foreach (var rtp in result)
            {
                rtp.RetrievalStatus = RetrievelStatusEnum.NEW;
                rtp.RetrievalRequestedTimestamp = DateTime.UtcNow;
                rtp.Rpt = rtp.AccessToken;
            }
        }

    }
}
