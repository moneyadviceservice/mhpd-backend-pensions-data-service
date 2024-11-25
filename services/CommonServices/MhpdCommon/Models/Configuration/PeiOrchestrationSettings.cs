namespace MhpdCommon.Models.Configuration;

public class PeiOrchestrationSettings
{
    public const string PeiRetrievalDurationVariable = nameof(PeiRetrievalDuration);
    public const string ViewDataDurationVariable = nameof(ViewDataRetrievalDuration);
    public const string PeiPollingIntervalVariable = nameof(PeiPollingInterval);
    public const int DefaultPeiRetrievalDuration = 60;
    public const int DefaultPeiPollingInterval = 5;
    public const int DefaultViewDataDuration = 5;

    public int PeiRetrievalDuration { get; set; } = DefaultPeiRetrievalDuration;

    public int PeiPollingInterval { get; set; } = DefaultPeiPollingInterval;

    public int ViewDataRetrievalDuration { get; set; } = DefaultViewDataDuration;

    public int RetryLimit => Math.Max((PeiRetrievalDuration / PeiPollingInterval), 1);

    public int TotalPensionRetrievalDuration => PeiRetrievalDuration + ViewDataRetrievalDuration;
}
