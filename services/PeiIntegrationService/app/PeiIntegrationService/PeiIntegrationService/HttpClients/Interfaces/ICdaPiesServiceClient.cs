using MhpdCommon.Models.MHPDModels;
using PeiIntegrationService.Models.CdaPeisServiceClient;
using PeiIntegrationService.Models.CdaPiesService;

namespace PeiIntegrationService.HttpClients.Interfaces
{
    public interface ICdaPiesServiceClient
    {
        Task<CdaPeisServiceResponseModel?> GetPiesAsync(CdaPiesServiceRequestModel request);
    }
}