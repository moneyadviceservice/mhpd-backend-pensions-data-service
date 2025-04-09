using CDAServiceEmulator.Models.Peis;
using MhpdCommon.Models.Configuration;
using MhpdCommon.Repository;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace CDAServiceEmulator.CosmosRepository;

public class CdaPeisEmulatorScenarioModelRepository(CosmosClient cosmosClient, IOptions<CosmosTestHarnessConfiguration> configuration)
    : CosmosDbRepository<CdaPeisEmulatorScenarioModel>(cosmosClient, configuration.Value.DatabaseName, configuration.Value.CdaPeisEmulatorScenarioModelContainerName), 
    ICdaPeisEmulatorScenarioModelRepository
{
    public async Task<int> GetMaxScenarioCodeAsync()
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