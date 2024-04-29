using TokenIntegrationService.Models;
namespace TokenIntegrationService.HttpClients
{
    public interface ICDAToken
    {
        Task<RptsModel[]?> PostRpts(CDATokenRequestModel request);
    }
    
}
