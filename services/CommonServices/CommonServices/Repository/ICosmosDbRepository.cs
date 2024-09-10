namespace MAPS.Core.Repository
{
    public interface ICosmosDbRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(string? id, string? partitionKey);
    }
}
