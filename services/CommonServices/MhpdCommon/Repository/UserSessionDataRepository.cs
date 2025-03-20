using MhpdCommon.Models.Configuration;
using MhpdCommon.Models.MHPDModels;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace MhpdCommon.Repository;

public class UserSessionDataRepository(CosmosClient cosmosClient, IOptions<CosmosBusinessConfiguration> options)
    : CosmosDbRepository<UserSessionData>(cosmosClient, options.Value.DatabaseId, options.Value.UserSessionDataContainer);
