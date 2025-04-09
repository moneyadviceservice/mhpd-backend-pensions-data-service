using CDAServiceEmulator.Models.ViewData;
using MhpdCommon.Models.Configuration;
using MhpdCommon.Repository;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace CDAServiceEmulator.CosmosRepository;

public class ViewDataRepository(CosmosClient cosmosClient, IOptions<CosmosTestHarnessConfiguration> configuration)
    : CosmosDbRepository<ViewDataPayloadModel>(cosmosClient, configuration.Value.DatabaseName, configuration.Value.ViewDataModelContainerName);
