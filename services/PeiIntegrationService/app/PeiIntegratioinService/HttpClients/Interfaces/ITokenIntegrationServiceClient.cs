using PeiIntegrationService.Models.TokenIntegrationService;

namespace PeiIntegrationService.HttpClients.Interfaces
{
    public interface ITokenIntegrationServiceClient
    {
        public Task<TokenIntegrationResponseModel> PostRpt(TokenIntegrationServiceRequestModel request);
    }
}
