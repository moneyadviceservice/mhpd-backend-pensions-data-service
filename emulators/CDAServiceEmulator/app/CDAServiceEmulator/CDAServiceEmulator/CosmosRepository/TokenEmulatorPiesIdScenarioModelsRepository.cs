using CDAServiceEmulator.Models.Token;
using MhpdCommon.Models.Configuration;
using MhpdCommon.Repository;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace CDAServiceEmulator.CosmosRepository;

public class TokenEmulatorPiesIdScenarioModelsRepository(CosmosClient cosmosClient, IOptions<CosmosTestHarnessConfiguration> configuration)
    : CosmosDbRepository<TokenEmulatorPiesIdScenarioModel>(cosmosClient, configuration.Value.DatabaseName, configuration.Value.TokenEmulatorPiesIdScenarioModelsContainerName);