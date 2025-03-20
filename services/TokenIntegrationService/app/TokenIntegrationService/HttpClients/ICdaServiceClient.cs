using MhpdCommon.Models.MessageBodyModels;

namespace TokenIntegrationService.HttpClients;

public interface ICdaServiceClient
{
    Task<CdaTokenResponseModel> PostAsync<TRequest>(TRequest request); 
}