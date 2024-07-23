using Microsoft.Azure.Cosmos;

namespace PDPViewDataServicedEmulator.CosmosRepository
{
    public class CosmosDbRepository<T> : ICosmosDbRepository<T> where T : class
    {
        private readonly Container _container;
        private readonly CosmosClient _cosmosClient;

        public CosmosDbRepository(CosmosClient cosmosClient, string databaseName, string containerName)
        {
            _cosmosClient = cosmosClient;
            var database = _cosmosClient.GetDatabase(databaseName);
            _container = database.GetContainer(containerName);
        }

        public async Task<T?> GetByIdAsync(string? id, string? partitionKey)
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
    }
}
