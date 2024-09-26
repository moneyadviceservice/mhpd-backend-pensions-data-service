using CDAServiceEmulator.Models.Token;
using MhpdCommon.Repository;
using Microsoft.Azure.Cosmos;

namespace CDAServiceEmulator.CosmosRepository;

public class TokenEmulatorPiesIdScenarioModelsRepository(CosmosClient cosmosClient, string databaseName, string containerName)
    : CosmosDbRepository<TokenEmulatorPiesIdScenarioModel>(cosmosClient, databaseName, containerName);