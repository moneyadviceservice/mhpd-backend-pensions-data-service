using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.Models.RequestHeaderModel;
using TokenIntegrationService.Models;

namespace TokenIntegrationService.HttpClients;

public interface ICdaServiceClient
{
    Task<RptsModel> PostRptAsync(CdaTokenRequestModel request, RequestHeaderModel requestHeader);
}