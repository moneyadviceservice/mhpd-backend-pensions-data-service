using CDAServiceEmulator.Models.ViewData;
using MhpdCommon.Repository;
using Microsoft.Azure.Cosmos;

namespace CDAServiceEmulator.CosmosRepository;

public class ViewDataRepository(CosmosClient cosmosClient, string databaseName, string containerName)
    : CosmosDbRepository<ViewDataPayloadModel>(cosmosClient, databaseName, containerName);
