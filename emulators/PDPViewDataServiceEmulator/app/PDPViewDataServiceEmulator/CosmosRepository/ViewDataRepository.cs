using MhpdCommon.Models.Configuration;
using MhpdCommon.Repository;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using PDPViewDataServiceEmulator.Mocks;

namespace PDPViewDataServiceEmulator.CosmosRepository;

public class ViewDataRepository(CosmosClient cosmosClient, IOptions<CosmosTestHarnessConfiguration> configuration)
    : CosmosDbRepository<ViewDataPayload>(cosmosClient, configuration.Value.DatabaseName, configuration.Value.ViewDataModelContainerName);