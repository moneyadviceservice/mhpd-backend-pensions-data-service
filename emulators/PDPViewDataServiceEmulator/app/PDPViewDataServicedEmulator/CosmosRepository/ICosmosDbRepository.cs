namespace PDPViewDataServicedEmulator.CosmosRepository
{
    public interface ICosmosDbRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(string? id, string? partitionKey);
    }
}
