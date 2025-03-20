namespace MhpdCommon.Models.Configuration;

public class CosmosBusinessConfiguration
{
    public string DatabaseId { get; set; } = string.Empty;

    public string UserSessionDataContainer { get; set; } = string.Empty;

    public string PensionsRetrievalContainer { get; set; } = string.Empty;

    public string RetrievedPensionsContainer { get; set; } = string.Empty;
}