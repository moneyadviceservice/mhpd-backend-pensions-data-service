namespace MhpdCommon.Models.Configuration;

public class CosmosIntegrationConfiguration
{
    public string DatabaseId { get; set; } = string.Empty;

    public string HolderNameCacheContainer { get; set; } = string.Empty;

    public string JwkCacheContainer { get; set; } = string.Empty;
}
