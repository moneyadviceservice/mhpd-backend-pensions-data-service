using MhpdCommon.Models.MHPDModels;
using MhpdCommon.Repository;
using Microsoft.Azure.Cosmos;

namespace CDAServiceEmulator.CosmosRepository;

public class HolderNameViewDataRepository(CosmosClient cosmosClient, string databaseName, string containerName)
        : CosmosDbRepository<HolderNameViewDataResponse>(cosmosClient, databaseName, containerName), IHolderNameViewDataRepository<HolderNameViewDataResponse>
{
    private readonly Container _container = cosmosClient.GetContainer(databaseName, containerName);

    public async Task<List<HolderNameViewDataResponse>> GetHolderNameConfigurationsAsync()
    {
        var query = new QueryDefinition("SELECT * FROM c");

        var iterator = _container.GetItemQueryIterator<HolderNameViewDataResponse>(query);

        var response = await iterator.ReadNextAsync();

        return [.. response];
    }
}
