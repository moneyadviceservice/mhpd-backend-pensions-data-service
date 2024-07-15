using System.Net.Http.Headers;

namespace PensionRequestFunction.HttpClient
{
    public  class PDPViewDataClient : IPDPViewDataClient
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public PDPViewDataClient(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<string> GetPDPViewDataAsync(string assetGuid, string viewDataUrl, string rpt)
        {
            var scope = "owner";
            var client = _httpClientFactory.CreateClient("PDPViewData");

            client.DefaultRequestHeaders.Add("X-Request-ID", Guid.NewGuid().ToString());
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", rpt);

            var response = await client.GetAsync($"{viewDataUrl}{assetGuid}?scope={scope}");

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to get PDP view data for item {assetGuid}");
            }

            return await response.Content.ReadAsStringAsync();
        }
    }
}