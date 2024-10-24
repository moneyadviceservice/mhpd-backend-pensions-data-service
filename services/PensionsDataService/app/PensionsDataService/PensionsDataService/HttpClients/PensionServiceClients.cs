namespace PensionsDataService.HttpClients;

public class PensionServiceClients(
    ITokenIntegrationServiceClient tokenIntegrationServiceClient,
    IRetrievalRecordServiceClient retrievalRecordServiceClient,
    IRetrievedPensionsRecordClient retrievedPensionsRecordClient)
{
    public ITokenIntegrationServiceClient TokenIntegrationServiceClient { get; } = tokenIntegrationServiceClient;
    public IRetrievalRecordServiceClient RetrievalRecordServiceClient { get; } = retrievalRecordServiceClient;
    public IRetrievedPensionsRecordClient RetrievedPensionsRecordClient { get; } = retrievedPensionsRecordClient;
}