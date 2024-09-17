namespace MhpdCommon.Repository;

public interface ICosmosDbRepository<T> where T : class
{
    Task<T?> GetByIdAsync(string id, string partitionKey);
    Task InsertItemAsync(T item, string partitionKey);
}


