using MhpdCommon.Models.MHPDModels;
using MhpdCommon.Models.RequestHeaderModel;

namespace MhpdCommon.SharedHttpClient;

public interface IMapsCdaServiceClient
{
    Task<MapsRqpServiceResponseModel> GetRqp(RequestHeaderModel request);
}