using CDAServiceEmulator.Models.HolderConfiguration;
using MhpdCommon.Repository;

namespace CDAServiceEmulator.CosmosRepository;

public interface IHolderNameViewDataRepository<T> : ICosmosDbRepository<T> where T : class
{
    Task<List<HolderNameConfigurationModel>> GetHolderNameConfigurationsAsync();
}
