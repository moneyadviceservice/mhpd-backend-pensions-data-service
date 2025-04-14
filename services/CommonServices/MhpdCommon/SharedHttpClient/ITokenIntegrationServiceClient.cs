using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.Models.MHPDModels;

namespace MhpdCommon.SharedHttpClient;

public interface ITokenIntegrationServiceClient
{
    public Task<CdaTokenResponseModel> PostAccessTokenAsync(TokenClientRequestModel request, string? correlationId = null);

    public Task<PeiRetrievalDetailsResponseModel> PostIdTokenAsync(PensionsDataRequestModel request, string? correlationId = null);
}
