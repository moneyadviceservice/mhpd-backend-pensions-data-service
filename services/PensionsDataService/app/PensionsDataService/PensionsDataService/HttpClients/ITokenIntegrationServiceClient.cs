using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.Models.RequestHeaderModel;
using PensionsDataService.Models;

namespace PensionsDataService.HttpClients;

public interface ITokenIntegrationServiceClient
{
    Task<PeiRetrievalDetailsResponseModel> PostAsync(CdaTokenRequestModel request, RequestHeaderModel requestHeader);
}