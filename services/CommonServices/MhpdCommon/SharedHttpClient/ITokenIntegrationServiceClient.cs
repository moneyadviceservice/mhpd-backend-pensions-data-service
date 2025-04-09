using MhpdCommon.Models.MessageBodyModels;

namespace MhpdCommon.SharedHttpClient;

public interface ITokenIntegrationServiceClient
{
    public Task<CdaTokenResponseModel> PostAccessTokenAsync(TokenClientRequestModel request, string? correlationId = null);

    public Task<CdaTokenResponseModel> PostIdTokenAsync(PensionsDataRequestModel request, string? correlationId = null);
}
