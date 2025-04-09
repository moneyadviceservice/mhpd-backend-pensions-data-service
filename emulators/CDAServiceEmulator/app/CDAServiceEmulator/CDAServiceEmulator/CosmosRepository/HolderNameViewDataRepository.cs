using MhpdCommon.Models.Configuration;
using MhpdCommon.Models.MHPDModels;
using MhpdCommon.Repository;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace CDAServiceEmulator.CosmosRepository;

public class HolderNameViewDataRepository(CosmosClient cosmosClient, IOptions<CosmosTestHarnessConfiguration> configuration)
        : CosmosDbRepository<HolderNameViewDataResponse>(cosmosClient, configuration.Value.DatabaseName, configuration.Value.HolderNameConfigurationModelsContainerName);
