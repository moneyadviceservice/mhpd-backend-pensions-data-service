using MhpdCommon.Models.MHPDModels.JwkUri;

namespace MhpdCommon.SharedHttpClient;

public interface ISharedHttpClient
{
    Task<JwkUriResponseModel> GetAsync();
}