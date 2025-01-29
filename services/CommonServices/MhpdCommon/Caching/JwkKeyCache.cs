using MhpdCommon.Models.Configuration;
using MhpdCommon.Models.MHPDModels.JwkUri;
using MhpdCommon.Repository;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace MhpdCommon.Caching;

public class JwkKeyCache(CosmosClient cosmosClient, IOptions<CosmosIntegrationConfiguration> options) :
CosmosDbRepository<JwkUriResponseModel>(cosmosClient, options.Value!.DatabaseId!, options.Value!.JwkCacheContainer!),
IJwkKeyCache<JwkUriResponseModel>
{
}
