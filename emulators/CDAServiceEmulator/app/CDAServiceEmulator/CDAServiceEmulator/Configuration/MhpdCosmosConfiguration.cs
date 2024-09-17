namespace CDAServiceEmulator.Configuration;

public class MhpdCosmosConfiguration
{
    public string DatabaseName { get; set; } = string.Empty;
    public string CdaPeisEmulatorScenarioModelContainerName { get; set; } = string.Empty;
    public string CdaPeisEmulatorTestInstanceDataContainerName { get; set; } = string.Empty;
    public string CdaPeisEmulatorScenarioModelContainerPartitionKey { get; set; } = string.Empty;
    public string CdaPeisEmulatorTestInstanceDataContainerPartitionKey { get; set; } = string.Empty;
}