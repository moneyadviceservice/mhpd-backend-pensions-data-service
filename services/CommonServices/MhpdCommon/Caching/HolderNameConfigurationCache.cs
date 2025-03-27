using MhpdCommon.Models.Configuration;
using MhpdCommon.Models.MHPDModels;
using MhpdCommon.Repository;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace MhpdCommon.Caching;

public class HolderNameConfigurationCache(CosmosClient cosmosClient, IOptions<CosmosIntegrationConfiguration> options) :
CosmosDbRepository<HolderNameViewDataResponse>(cosmosClient, options.Value!.DatabaseId!, options.Value!.HolderNameCacheContainer!),
IHolderNameConfigurationCache<HolderNameViewDataResponse>
{
}
