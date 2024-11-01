using Azure.Messaging.ServiceBus;
using MhpdCommon.Extensions;
using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.Utils;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using PensionsRetrievalFunction.Models;
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
        if (!_idValidator.IsValidGuid(message.CorrelationId))
        {
            _logger.LogCritical("Missing or Invalid correlationId: {correlationId}", message.CorrelationId);
            await messageActions.DeadLetterMessageAsync(message, deadLetterReason: $"Missing or Invalid correlationId: {message.CorrelationId}");
            return;
        }

        using var scope = _logger.BeginCorrelationScope(message.CorrelationId, Constants.LogSource.Queue);
        LogRequestMesage(message);

        try
        {
            var payload = ExtractAndValidateMessagePayload(message);

            // Release the lock on the message
            await messageActions.CompleteMessageAsync(message);

            await _orchestrator.RunAsync(payload, message.CorrelationId);
        }
        catch (Exception error)
        {
            _logger.LogCritical(error, "{message}", error.Message);
            await messageActions.DeadLetterMessageAsync(message, deadLetterReason: error.Message);
        }
    }

    private PensionRetrievalPayload ExtractAndValidateMessagePayload(ServiceBusReceivedMessage message)
    {
        var messageBody = Encoding.UTF8.GetString(message.Body);
        string? logMessage;
        PensionRetrievalPayload? payload;

        try
        {
            payload = _messageParser.ToPensionRetrievalPayload(messageBody);
        }
        catch (AggregateException error)
        {
            var builder = new StringBuilder(Constants.ResponseType.InvalidPayloadResponse);
            builder.AppendLine();
            foreach (var ex in error.InnerExceptions)
            {
                builder.AppendLine(ex.Message);
            }

            logMessage = builder.ToString();
            throw new InvalidDataException(logMessage, error);
        }

        return payload!;
    }

    private void LogRequestMesage(ServiceBusReceivedMessage receivedMessage)
    {
        var logMessage = $"Message Received - CorrelationId:[{receivedMessage.CorrelationId}], " +
            $"MessageId: [{receivedMessage.MessageId}], ContentType: [{receivedMessage.ContentType}] {Environment.NewLine}";
        _logger.LogWarning("Message Details : {details} Body: {body}", logMessage, receivedMessage.Body);
    }
}
