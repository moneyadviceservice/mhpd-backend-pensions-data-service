using MhpdCommon.Models.MHPDModels;
using MhpdCommon.Repository;
using Microsoft.Azure.Cosmos;

namespace PensionRequestFunction.Repository;

public class HolderNameConfigurationRepository(CosmosClient cosmosClient, string databaseName, string containerName) : 
    CosmosDbRepository<HolderNameConfigurationModel>(cosmosClient, databaseName, containerName), IHolderNameConfigurationRepository<HolderNameConfigurationModel>
{
}
