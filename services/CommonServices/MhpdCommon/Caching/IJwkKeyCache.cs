using MhpdCommon.Repository;

namespace MhpdCommon.Caching;

public interface IJwkKeyCache<T> : ICosmosDbRepository<T> where T : class
{
}
