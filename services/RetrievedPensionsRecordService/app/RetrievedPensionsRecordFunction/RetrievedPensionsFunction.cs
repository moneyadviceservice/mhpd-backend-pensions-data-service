using Azure.Messaging.ServiceBus;
using MhpdCommon.Extensions;
using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.Utils;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using RetrievedPensionsRecordFunction.Models;
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
    private const string InvalidPayloadResponse = "Invalid retrieved pension payload";

    [Function(nameof(RetrievedPensionsFunction))]
    public async Task Run(
        [ServiceBusTrigger("%InboundQueue%", Connection = "ServiceBusConnectionstring")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions)
    {
        if (!_idValidator.IsValidGuid(message.CorrelationId))
        {
            var Idresponse = $"Missing or Invalid correlationId: {message.CorrelationId}";
            _logger.LogCritical(Idresponse);
            await messageActions.DeadLetterMessageAsync(message, deadLetterReason: Idresponse);
            return;
        }

        using var scope = _logger.BeginCorrelationScope(message.CorrelationId, Constants.QueueLogSource);
        LogRequestMesage(message);

        try
        {
            var payload = ExtractAndValidateMessagePayload(message);

            if (await _pensionRepository.SaveRetrievedPensionRecordAsync(message.CorrelationId, payload))
            {
                await messageActions.CompleteMessageAsync(message);
            }
            else
            {
                await messageActions.AbandonMessageAsync(message);
            }
        }
        catch (Exception error)
        {
            _logger.LogCritical(error, error.Message);
            await messageActions.DeadLetterMessageAsync(message, deadLetterReason: error.Message);
        }
    }

    private RetrievedPensionDetailsPayload ExtractAndValidateMessagePayload(ServiceBusReceivedMessage message)
    {
        var messageBody = Encoding.UTF8.GetString(message.Body);
        string? logMessage;
        RetrievedPensionDetailsPayload? payload;

        try
        {
            payload = _messageParser.ToRetrievedPensionPayload(messageBody);
        }
        catch (AggregateException error)
        {
            var builder = new StringBuilder(InvalidPayloadResponse);
            builder.AppendLine();
            foreach (var ex in error.InnerExceptions)
            {
                builder.AppendLine(ex.Message);
            }

            logMessage = builder.ToString();
            throw new InvalidDataException(logMessage, error);
        }

        if (!_pensionValidator.ValidateRecord(payload, out var reason))
        {
            throw new InvalidDataException($"{InvalidPayloadResponse} - {reason}");
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
