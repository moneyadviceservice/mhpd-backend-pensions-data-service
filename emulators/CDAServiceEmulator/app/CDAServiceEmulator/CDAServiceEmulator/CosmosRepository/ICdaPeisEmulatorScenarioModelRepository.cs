using CDAServiceEmulator.Models.Peis;
using MhpdCommon.Repository;

namespace CDAServiceEmulator.CosmosRepository;

public interface ICdaPeisEmulatorScenarioModelRepository : ICosmosDbRepository<CdaPeisEmulatorScenarioModel>
{
    Task<int> GetMaxScenarioCodeAsync();
}
