using Azure.Messaging.ServiceBus;
using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.Utils;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using RetrievedPensionsRecordFunction.Repository;
using RetrievedPensionsRecordFunction.Utils;
using System.Text;

namespace RetrievedPensionsRecordFunction;

public class RetrievedPensionsFunction(ILogger<RetrievedPensionsFunction> logger,
    IIdValidator idValidator,
    IMessageParser messageParser,
    IPensionRecordValidator pensionValidator,
    IPensionRecordRepository pensionRepository)
{
    private readonly ILogger<RetrievedPensionsFunction> _logger = logger;
    private readonly IIdValidator _idValidator = idValidator;
    private readonly IMessageParser _messageParser = messageParser;
    private readonly IPensionRecordValidator _pensionValidator = pensionValidator;
    private readonly IPensionRecordRepository _pensionRepository = pensionRepository;

    [Function(nameof(RetrievedPensionsFunction))]
    public async Task Run(
        [ServiceBusTrigger("%InboundQueue%", Connection = "ServiceBusConnectionstring")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions)
    {
        LogRequestMesage(message);

        if (!_idValidator.IsValidGuid(message.CorrelationId))
        {
            var Idresponse = $"Missing or Invalid correlationId: {message.CorrelationId}";
            _logger.LogCritical(Idresponse);
            await messageActions.DeadLetterMessageAsync(message, deadLetterReason: Idresponse);
            return;
        }

        const string payloadResponse = "Invalid retrieved pension payload";
        var messageBody = Encoding.UTF8.GetString(message.Body);
        RetrievedPensionDetailsPayload? payload;
        string? logMessage;

        try
        {
            payload = _messageParser.ToRetrievedPensionPayload(messageBody);
        }
        catch (AggregateException error)
        {
            var builder = new StringBuilder(payloadResponse);
            builder.AppendLine();
            foreach (var ex in error.InnerExceptions)
            {
                builder.AppendLine(ex.Message); 
            }

            logMessage = builder.ToString();
            _logger.LogCritical(error, logMessage);
            await messageActions.DeadLetterMessageAsync(message, deadLetterReason: logMessage);
            return;
        }

        if (!_pensionValidator.ValidateRecord(payload, out var reason))
        {
            _logger.LogCritical(reason);
            await messageActions.DeadLetterMessageAsync(message, deadLetterReason: reason);
            return;
        }

        if (payload == null)
        {
            _logger.LogCritical(payloadResponse);
            await messageActions.DeadLetterMessageAsync(message, deadLetterReason: payloadResponse);
            return;
        }

        bool result = await _pensionRepository.SaveRetrievedPensionRecordAsync(message.CorrelationId, payload);

        if (result)
        {
            logMessage = $"Retrieved pension successfully saved: {payload.PensionRetrievalRecordId}";
            _logger.LogInformation(logMessage);
            await messageActions.CompleteMessageAsync(message);
        }
        else
        {
            logMessage = $"Failed to persist retrieved pension: {payload.PensionRetrievalRecordId}";
            _logger.LogCritical(logMessage);
            await messageActions.AbandonMessageAsync(message);
        }
    }

    private void LogRequestMesage(ServiceBusReceivedMessage receivedMessage)
    {
        var logMessage = $"Message Received - CorrelationId:[{receivedMessage.CorrelationId}], " +
            $"MessageId: [{receivedMessage.MessageId}], ContentType: [{receivedMessage.ContentType}]";
        _logger.LogInformation(logMessage);
        _logger.LogInformation("Message Body: {body}", receivedMessage.Body);
    }
}
