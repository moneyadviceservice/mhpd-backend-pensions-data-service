using MhpdCommon.Repository;

namespace PensionRequestFunction.Repository;

public interface IHolderNameConfigurationRepository<T> : ICosmosDbRepository<T> where T : class
{
}
