namespace PensionsRetrievalFunction.Models;

public class PeiOrchestrationSettings
{
    public const string PeiRetryTimeoutVariable = nameof(PeiRetryTimeout);
    public const string PeiRetryIntervalVariable = nameof(PeiRetryInterval);
    public const int MaxRetryDuration = 60;
    public const int MinRetryInterval = 5;

    public int PeiRetryTimeout { get; set; } = MaxRetryDuration;

    public int PeiRetryInterval { get; set; } = MinRetryInterval;

    public int RetryLimit => Math.Max((PeiRetryTimeout / PeiRetryInterval), 1);
}
