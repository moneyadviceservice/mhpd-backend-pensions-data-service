using MhpdCommon.Repository;

namespace MhpdCommon.Caching;

public interface IHolderNameConfigurationCache<T> : ICosmosDbRepository<T> where T : class
{
}
