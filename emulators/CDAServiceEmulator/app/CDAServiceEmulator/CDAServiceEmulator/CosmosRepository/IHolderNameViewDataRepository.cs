using MhpdCommon.Models.MHPDModels;
using MhpdCommon.Repository;

namespace CDAServiceEmulator.CosmosRepository;

public interface IHolderNameViewDataRepository<T> : ICosmosDbRepository<T> where T : class
{
    Task<List<HolderNameViewDataResponse>> GetHolderNameConfigurationsAsync();
}
