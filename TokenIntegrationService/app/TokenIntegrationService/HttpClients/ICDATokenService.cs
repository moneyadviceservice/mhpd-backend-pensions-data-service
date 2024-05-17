using TokenIntegrationService.Models;

namespace TokenIntegrationService.HttpClients
{
    public interface ICDATokenService
    {
        Task<RptsModel> PostRpt(CDATokenRequestModel request);
    }
}
