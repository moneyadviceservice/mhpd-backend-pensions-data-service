using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace PensionsRetrievalFunction;

public class RetrievalFunction(ILogger<RetrievalFunction> logger)
{
    private readonly ILogger<RetrievalFunction> _logger = logger;

    [Function(nameof(RetrievalFunction))]
    public async Task Run(
        [ServiceBusTrigger("mhpd-pensions-retrieval-job-sb-queue-dev", Connection = "ServiceBusConnectionstring")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions)
    {
        LogRequestMesage(message);

        // Complete the message
        await messageActions.CompleteMessageAsync(message);
    }

    private void LogRequestMesage(ServiceBusReceivedMessage receivedMessage)
    {
        var logMessage = $"Message Received - CorrelationId:[{receivedMessage.CorrelationId}], " +
            $"MessageId: [{receivedMessage.MessageId}], ContentType: [{receivedMessage.ContentType}]";
        _logger.LogInformation("{Message}", logMessage);
        _logger.LogInformation("Message Body: {body}", receivedMessage.Body);
    }
}
