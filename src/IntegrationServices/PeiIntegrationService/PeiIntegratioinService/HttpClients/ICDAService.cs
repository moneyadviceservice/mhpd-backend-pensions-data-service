using PeiIntegrationService.Models;

namespace PeiIntegratioinService.HttpClients
{
    public interface ICDAService
    {
        Task<PeiModel[]?> GetPies(CDAServiceRequestModel request);
    }
}