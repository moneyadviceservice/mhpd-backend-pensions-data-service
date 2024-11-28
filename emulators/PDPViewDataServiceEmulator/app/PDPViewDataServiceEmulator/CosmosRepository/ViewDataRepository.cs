using MhpdCommon.Repository;
using Microsoft.Azure.Cosmos;
using PDPViewDataServiceEmulator.Mocks;

namespace PDPViewDataServiceEmulator.CosmosRepository;

public class ViewDataRepository(CosmosClient cosmosClient, string databaseName, string containerName)
    : CosmosDbRepository<ViewDataPayload>(cosmosClient, databaseName, containerName);