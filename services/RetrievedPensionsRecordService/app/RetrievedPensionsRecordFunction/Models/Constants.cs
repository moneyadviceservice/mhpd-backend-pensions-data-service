namespace RetrievedPensionsRecordFunction.Models;

public static class Constants
{
    public const string QueueLogSource = "Retrieved Pensions Message Queue";
    public const string HttpLogSource = "Retrieved Pensions Http GET";
    public const string RetrievedRecordQuery = "pensionsRetrievalRecordId";
    public const string InvalidRecordId = "pensionsRetrievalRecordId missing or invalid";
    public const string InvalidCorrelationId = "mhpdCorrelationId invalid";
}
