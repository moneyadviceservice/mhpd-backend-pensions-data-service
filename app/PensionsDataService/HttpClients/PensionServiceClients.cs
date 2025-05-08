using MhpdCommon.Models.Configuration;
using MhpdCommon.SharedHttpClient;
using MhpdCommon.Utils;
using Microsoft.Extensions.Options;

namespace PensionsDataService.HttpClients;

public class PensionServiceClients(
    ITokenIntegrationServiceClient tokenIntegrationServiceClient,
    IRetrievalRecordServiceClient retrievalRecordServiceClient,
    IRetrievedPensionsRecordClient retrievedPensionsRecordClient,
    IMapsCdaServiceClient mapsCdaServiceClient,
    IOptions<CommonServiceBusConfiguration> serviceBusOptions,
    IMessagingService messagingService)
{
    public ITokenIntegrationServiceClient TokenIntegrationServiceClient { get; } = tokenIntegrationServiceClient;
    public IRetrievalRecordServiceClient RetrievalRecordServiceClient { get; } = retrievalRecordServiceClient;
    public IRetrievedPensionsRecordClient RetrievedPensionsRecordClient { get; } = retrievedPensionsRecordClient;
    public IMapsCdaServiceClient MapsCdaServiceClient { get; } = mapsCdaServiceClient;
    public IOptions<CommonServiceBusConfiguration> ServiceBusOptions { get; } = serviceBusOptions;
    public IMessagingService MessagingService { get; } = messagingService;
}