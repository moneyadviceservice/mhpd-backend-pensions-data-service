using MhpdCommon.Models.RequestHeaderModel;
using TokenIntegrationService.Models;

namespace TokenIntegrationService.HttpClients;

public interface ICdaServiceClient
{
    Task<CdaTokenResponseModel> PostAsync<TRequest>(TRequest request, RequestHeaderModel requestHeader);
}