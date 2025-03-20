using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.Models.MHPDModels;

namespace MhpdCommon.SharedHttpClient;

public interface IPeiServiceClient
{
    Task<CdaPeisServiceResponseModel> GetPeiDataAsync(PeiRequestModel request);
}
