using PeiIntegrationService.Models;

namespace PeiIntegratioinService.HttpClients
{
    public class CDAService : ICDAService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public CDAService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<PeiModel[]?> GetPies(CDAServiceRequestModel request)
        {
            var client = _httpClientFactory.CreateClient("CDAService");

            client.DefaultRequestHeaders.Add("Authorisation", request.Authorization);
            client.DefaultRequestHeaders.Add("X-Version", "1.1"); // <============ hardcoded for now
            client.DefaultRequestHeaders.Add("X-Request-ID", request.RequestId);
            client.BaseAddress = new Uri(request.CdaServiceUrl!);

            var endPoint = $"peis/{request.CdaUserGuid}";

            var response =  await client!.GetAsync(endPoint);

            var result = await response.Content.ReadFromJsonAsync<PeiModel[]>();
            ApplyRetrievalStatus(ref result!);
            return result;

        }

        private void ApplyRetrievalStatus(ref PeiModel[] result)
        {
            foreach (var pei in result)
            {
                pei.RetrievalStatus = RetrievelStatusEnum.NEW;
                pei.RetrievalRequestedTimestamp = DateTime.UtcNow;
            }
        }
    }
}
