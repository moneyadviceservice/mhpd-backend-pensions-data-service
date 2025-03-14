using CDAServiceEmulator.Models.Peis;
using MhpdCommon.Repository;
using Microsoft.Azure.Cosmos;
using System.Diagnostics.CodeAnalysis;

namespace CDAServiceEmulator.CosmosRepository;

[ExcludeFromCodeCoverage]
public class CdaPeisEmulatorScenarioModelRepository(CosmosClient cosmosClient, string databaseName, string containerName)
    : CosmosDbRepository<CdaPeisEmulatorScenarioModel>(cosmosClient, databaseName, containerName)
{
    public async Task<int> GetMaxPartitionKeyAsync()
    {
        var query = new QueryDefinition("SELECT MAX(c.peisIdStartCode) AS MaxStartCode FROM c");

        using FeedIterator<dynamic> resultSet = Container.GetItemQueryIterator<dynamic>(query);

        if (resultSet.HasMoreResults)
        {
            FeedResponse<dynamic> response = await resultSet.ReadNextAsync();
            var maxPartitionKey = response.FirstOrDefault()?.MaxStartCode?.ToString();
            return int.Parse(maxPartitionKey);
        }

        return 0;
    }
}