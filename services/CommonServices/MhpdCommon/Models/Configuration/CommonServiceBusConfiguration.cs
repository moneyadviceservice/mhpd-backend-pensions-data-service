namespace MhpdCommon.Models.Configuration;

public class CommonServiceBusConfiguration
{
    public const string ConnectionStringVariable = "ServiceBusConnectionString";
    public const string InboundQueueVariable = nameof(InboundQueue);
    public const string OutboundQueueVariable = nameof(OutboundQueue);

    public string? InboundQueue { get; set; }
    public string? OutboundQueue { get; set; }
}
