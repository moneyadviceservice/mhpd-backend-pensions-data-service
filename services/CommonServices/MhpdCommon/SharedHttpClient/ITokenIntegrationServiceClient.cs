using MhpdCommon.Models.MessageBodyModels;

namespace MhpdCommon.SharedHttpClient;

public interface ITokenIntegrationServiceClient
{
    public Task<CdaTokenResponseModel> PostRptAsync(TokenClientRequestModel request);
}
