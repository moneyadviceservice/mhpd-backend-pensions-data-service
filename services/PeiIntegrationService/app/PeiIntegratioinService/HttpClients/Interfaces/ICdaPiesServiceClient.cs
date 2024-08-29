using PeiIntegrationService.Models.CdaPiesService;

namespace PeiIntegrationService.HttpClients.Interfaces
{
    public interface ICdaPiesServiceClient
    {
        Task<CdaPiesServiceResponseModel?> GetPiesAsync(CdaPiesServiceRequestModel request);
    }
}