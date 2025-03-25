using MhpdCommon.SharedHttpClient;
using PensionRequestFunction.HttpClient;

namespace PensionRequestFunction.Orchestration;

public class ViewDataOrchestratorClients(
    IHolderNameClient holderNameClient,
    IPdpViewDataClient viewDataClient,
    ITokenIntegrationServiceClient tokenClient,
    IMapsCdaServiceClient rqpClient)
{
    public IHolderNameClient HolderNameClient { get; } = holderNameClient;
    public IPdpViewDataClient ViewDataClient { get; } = viewDataClient;
    public ITokenIntegrationServiceClient TokenClient { get; } = tokenClient;
    public IMapsCdaServiceClient RqpClient { get; } = rqpClient;
}