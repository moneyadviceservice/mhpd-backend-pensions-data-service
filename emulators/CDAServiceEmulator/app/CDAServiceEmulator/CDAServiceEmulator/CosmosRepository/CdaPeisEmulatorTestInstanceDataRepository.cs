using CDAServiceEmulator.Models.Peis;
using MhpdCommon.Repository;
using Microsoft.Azure.Cosmos;

namespace CDAServiceEmulator.CosmosRepository;

public class CdaPeisEmulatorTestInstanceDataRepository(
    CosmosClient cosmosClient,
    string databaseName,
    string containerName)
    : CosmosDbRepository<CdaPeisEmulatorTestInstanceDataModel>(cosmosClient, databaseName, containerName);
