using PensionRequestFunction.Models.TokenIntegrationServiceClient;

namespace PensionRequestFunction.HttpClient.Interfaces
{
    public interface ITokenIntegrationServiceClient
    {
        public Task<TokenIntegrationResponseModel> PostRptAsync(TokenIntegrationServiceRequestModel request);
    }
}
