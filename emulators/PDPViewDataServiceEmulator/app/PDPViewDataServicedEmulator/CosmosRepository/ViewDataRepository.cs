using Microsoft.Azure.Cosmos;
using PDPViewDataServicedEmulator.Mocks;

namespace PDPViewDataServicedEmulator.CosmosRepository;

public class ViewDataRepository(CosmosClient cosmosClient, string databaseName, string containerName)
    : CosmosDbRepository<ViewDataPayload>(cosmosClient, databaseName, containerName);