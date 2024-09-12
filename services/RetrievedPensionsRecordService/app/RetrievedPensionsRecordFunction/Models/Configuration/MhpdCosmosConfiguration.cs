namespace RetrievedPensionsRecordFunction.Models.Configuration;

public class MhpdCosmosConfiguration
{
    public string DatabaseId { get; set; } = string.Empty;
    public string ContainerId { get; set; } = string.Empty;
    public string ContainerPartitionKey { get; set; } = string.Empty;
}
