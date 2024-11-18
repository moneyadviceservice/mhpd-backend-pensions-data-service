using PeiIntegrationService.Models.MapsCdaService;

namespace PeiIntegrationService.HttpClients.Interfaces
{
    public interface IMapsRqpServiceClient
    {
        Task<MapsRqpServiceResponseModel> PostRqp(MapsRqpServiceRequestModel request);
    }
}