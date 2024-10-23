using MhpdCommon.Models.Configuration;
using MhpdCommon.Models.MHPDModels;
using MhpdCommon.Repository;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace PensionRequestFunction.Repository;

public class HolderNameConfigurationRepository(CosmosClient cosmosClient, IOptions<CommonCosmosConfiguration> options) : 
    CosmosDbRepository<HolderNameConfigurationModel>(cosmosClient, options.Value!.DatabaseId!, options.Value!.ContainerId!), 
    IHolderNameConfigurationRepository<HolderNameConfigurationModel>
{
}
