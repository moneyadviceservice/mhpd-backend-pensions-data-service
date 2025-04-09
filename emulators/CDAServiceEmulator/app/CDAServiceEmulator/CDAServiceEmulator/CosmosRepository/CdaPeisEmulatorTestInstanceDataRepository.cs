using CDAServiceEmulator.Models.Peis;
using MhpdCommon.Models.Configuration;
using MhpdCommon.Repository;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace CDAServiceEmulator.CosmosRepository;

public class CdaPeisEmulatorTestInstanceDataRepository(
    CosmosClient cosmosClient,
    IOptions<CosmosTestHarnessConfiguration> configuration)
    : CosmosDbRepository<CdaPeisEmulatorTestInstanceDataModel>(cosmosClient, configuration.Value.DatabaseName, configuration.Value.CdaPeisEmulatorTestInstanceDataContainerName);
