using Azure.Messaging.ServiceBus;
using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.Utils;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using PensionsRetrievalFunction.Orchestration;
using System.Text;

namespace PensionsRetrievalFunction;

public class RetrievalFunction(ILogger<RetrievalFunction> logger,
    IIdValidator idValidator,
    IMessageParser messageParser,
    IPeiIntegrationOrchestrator orchestrator)
{
    private readonly ILogger<RetrievalFunction> _logger = logger;
    private readonly IIdValidator _idValidator = idValidator;
    private readonly IMessageParser _messageParser = messageParser;
    private readonly IPeiIntegrationOrchestrator _orchestrator = orchestrator;

    [Function(nameof(RetrievalFunction))]
    public async Task Run(
        [ServiceBusTrigger("%InboundQueue%", Connection = "ServiceBusConnectionstring")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions)
    {
        LogRequestMesage(message);

        if (!_idValidator.IsValidGuid(message.CorrelationId))
        {
            _logger.LogCritical("Missing or Invalid correlationId: {correlationId}", message.CorrelationId);
            await messageActions.DeadLetterMessageAsync(message, deadLetterReason: $"Missing or Invalid correlationId: {message.CorrelationId}");
            return;
        }

        const string payloadResponse = "Invalid pension retrieval payload";
        var messageBody = Encoding.UTF8.GetString(message.Body);
        PensionRetrievalPayload? payload;

        try
        {
            payload = _messageParser.ToPensionRetrievalPayload(messageBody);
        }
        catch (AggregateException error)
        {
            var builder = new StringBuilder(payloadResponse);
            builder.AppendLine();
            foreach (var ex in error.InnerExceptions)
            {
                builder.AppendLine(ex.Message);
            }

            var logMessage = builder.ToString();
            _logger.LogError(error, "{logMessage}", logMessage);
            await messageActions.DeadLetterMessageAsync(message, deadLetterReason: logMessage);
            return;
        }

        if (payload == null)
        {
            _logger.LogCritical(payloadResponse);
            await messageActions.DeadLetterMessageAsync(message, deadLetterReason: payloadResponse);
            return;
        }

        // Release the lock on the message
        await messageActions.CompleteMessageAsync(message);

        await _orchestrator.RunAsync(payload, message.CorrelationId); 
    }

    private void LogRequestMesage(ServiceBusReceivedMessage receivedMessage)
    {
        var logMessage = $"Message Received - CorrelationId:[{receivedMessage.CorrelationId}], " +
            $"MessageId: [{receivedMessage.MessageId}], ContentType: [{receivedMessage.ContentType}]";
        _logger.LogInformation("{Message}", logMessage);
        _logger.LogInformation("Message Body: {body}", receivedMessage.Body);
    }
}
