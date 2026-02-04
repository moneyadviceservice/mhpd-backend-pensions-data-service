using MhpdCommon.Models.Configuration;
using MhpdCommon.SharedHttpClient;
using Microsoft.Extensions.Options;

namespace PensionsDataService.HttpClients;

public class PensionServiceClients(
    ITokenIntegrationServiceClient tokenIntegrationServiceClient,
    IRetrievalRecordServiceClient retrievalRecordServiceClient,
    IRetrievedPensionsRecordClient retrievedPensionsRecordClient,
    IMapsCdaServiceClient mapsCdaServiceClient,
    IOptions<CommonServiceBusConfiguration> serviceBusOptions)
{
    public ITokenIntegrationServiceClient TokenIntegrationServiceClient { get; } = tokenIntegrationServiceClient;
    public IRetrievalRecordServiceClient RetrievalRecordServiceClient { get; } = retrievalRecordServiceClient;
    public IRetrievedPensionsRecordClient RetrievedPensionsRecordClient { get; } = retrievedPensionsRecordClient;
    public IMapsCdaServiceClient MapsCdaServiceClient { get; } = mapsCdaServiceClient;
    public IOptions<CommonServiceBusConfiguration> ServiceBusOptions { get; } = serviceBusOptions;
}