using Microsoft.Azure.Cosmos;

namespace MhpdCommon.Repository;

public class CosmosDbRepository<T> : ICosmosDbRepository<T> where T : class
{
    private readonly Container _container;

    public CosmosDbRepository(CosmosClient cosmosClient, string databaseName, string containerName)
    {
        var database = cosmosClient.GetDatabase(databaseName);
        _container = database.GetContainer(containerName);
    }

    protected Container Container { get { return _container; } }

    public async Task<T?> GetByIdAsync(string id, string partitionKey)
    {
        try
        {
            var response = await _container.ReadItemAsync<T>(id, new PartitionKey(partitionKey));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return default;
        }
    }
    
    public async Task InsertItemAsync(T item, string partitionKey)
    {
        ArgumentNullException.ThrowIfNull(item);
        ArgumentNullException.ThrowIfNull(partitionKey);

        try
        {
            await _container.UpsertItemAsync(item, new PartitionKey(partitionKey));
        }
        catch (CosmosException ex)
        {
            throw new Exception("Failed to upsert item.", ex);
        }
    }
}
