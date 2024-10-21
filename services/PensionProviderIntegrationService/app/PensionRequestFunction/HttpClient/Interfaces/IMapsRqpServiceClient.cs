using PensionRequestFunction.Models.MapsRqpServiceClient;

namespace PensionRequestFunction.HttpClient.Interfaces
{
    public interface IMapsCdaServiceClient
    {
        Task<MapsRqpServiceResponseModel> PostRqpAsync(MapsRqpServiceRequestModel request);
    }
}
