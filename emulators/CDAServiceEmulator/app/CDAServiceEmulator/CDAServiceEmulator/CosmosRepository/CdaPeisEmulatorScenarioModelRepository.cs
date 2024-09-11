using CDAServiceEmulator.Models.Peis;
using MhpdCommon.Repository;
using Microsoft.Azure.Cosmos;

namespace CDAServiceEmulator.CosmosRepository;

public class CdaPeisEmulatorScenarioModelRepository(CosmosClient cosmosClient, string databaseName, string containerName)
    : CosmosDbRepository<CdaPeisEmulatorScenarioModel>(cosmosClient, databaseName, containerName);