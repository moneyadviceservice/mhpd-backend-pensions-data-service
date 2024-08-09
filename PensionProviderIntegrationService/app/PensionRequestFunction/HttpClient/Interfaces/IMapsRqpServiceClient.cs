using PensionRequestFunction.Models.MapsRqpServiceClient;

namespace PensionRequestFunction.HttpClient.Interfaces
{
    public interface IMapsRqpServiceClient
    {
        Task<MapsRqpServiceResponseModel> PostRqp(MapsRqpServiceRequestModel request);
    }
}
