using CDAServiceEmulator.Models.HolderConfiguration;
using MhpdCommon.Repository;
using Microsoft.Azure.Cosmos;

namespace CDAServiceEmulator.CosmosRepository;

public class HolderNameViewDataRepository(CosmosClient cosmosClient, string databaseName, string containerName)
        : CosmosDbRepository<HolderNameConfigurationModel>(cosmosClient, databaseName, containerName), IHolderNameViewDataRepository<HolderNameConfigurationModel>
{
    private readonly Container _container = cosmosClient.GetContainer(databaseName, containerName);

    public async Task<List<HolderNameConfigurationModel>> GetHolderNameConfigurationsAsync()
    {
        var query = new QueryDefinition("SELECT * FROM c");

        var iterator = _container.GetItemQueryIterator<HolderNameConfigurationModel>(query);

        var response = await iterator.ReadNextAsync();

        return [.. response];
    }
}
