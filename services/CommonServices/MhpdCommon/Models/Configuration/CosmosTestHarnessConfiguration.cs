namespace MhpdCommon.Models.Configuration;

public class CosmosTestHarnessConfiguration
{
    public string DatabaseName { get; set; } = string.Empty;

    public string CdaPeisEmulatorScenarioModelContainerName { get; set; } = string.Empty;

    public string CdaPeisEmulatorTestInstanceDataContainerName { get; set; } = string.Empty;

    public string TokenEmulatorPiesIdScenarioModelsContainerName { get; set; } = string.Empty;

    public string HolderNameConfigurationModelsContainerName { get; set; } = string.Empty;

    public string ViewdatapayloadsContainerName { get; set; } = string.Empty;
}
