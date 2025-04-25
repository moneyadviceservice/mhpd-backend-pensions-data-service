namespace MhpdCommon.Repository;

public interface ICosmosDbRepository<T> where T : class
{
    Task<T?> GetByIdAsync(string id, string partitionKey);

    Task<T?> GetByIdStreamAsync(string id, string partitionKey);

    Task InsertItemAsync(T item, string partitionKey);

    Task<IEnumerable<T>> GetAllAsync();

    Task<bool> DeleteByIdAsync(string id, string partitionKey);
}


