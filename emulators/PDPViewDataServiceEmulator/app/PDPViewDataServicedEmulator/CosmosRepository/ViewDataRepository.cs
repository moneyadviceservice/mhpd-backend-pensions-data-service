using Microsoft.Azure.Cosmos;
using PDPViewDataServicedEmulator.Mocks;

namespace PDPViewDataServicedEmulator.CosmosRepository
{
    public class ViewDataRepository : CosmosDbRepository<ViewDataPayload>
    {
        public ViewDataRepository(CosmosClient cosmosClient, string databaseName, string containerName) : 
            base(cosmosClient, databaseName, containerName) { }
    }
}