using MhpdCommon.Models.MHPDModels;
using MhpdCommon.Repository;
using Microsoft.Azure.Cosmos;

namespace PensionsDataService.CosmosRepository;

public class UserSessionDataRepository(CosmosClient cosmosClient, string databaseName, string containerName)
    : CosmosDbRepository<UserSessionData>(cosmosClient, databaseName, containerName);