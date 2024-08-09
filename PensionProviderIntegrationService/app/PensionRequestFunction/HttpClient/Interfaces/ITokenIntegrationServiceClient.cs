using PensionRequestFunction.Models.TokenIntegrationServiceClient;

namespace PensionRequestFunction.HttpClient.Interfaces
{
    public interface ITokenIntegrationServiceClient
    {
        public Task<TokenIntegrationResponseModel> PostRpt(TokenIntegrationServiceRequestModel request);
    }
}
