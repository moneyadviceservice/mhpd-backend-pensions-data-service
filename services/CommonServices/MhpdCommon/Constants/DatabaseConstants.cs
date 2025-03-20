namespace MhpdCommon.Constants;

public static class DatabaseConstants
{
    public const string ConnectionStringVariable = "CosmosDBConnectionString";

    public static class ConfigurationSections
    {
        public const string BusinessLayer = "CosmosBusinessConfiguration";
        public const string IntegrationLayer = "CosmosIntegrationConfiguration";
        public const string TestHarness = "CosmosTestHarnessConfiguration";
    }
}
