using Azure.Messaging.ServiceBus;
using System.Text.Json;

namespace MhpdCommon.Utils;

public class MessagingService(ServiceBusClient serviceBusClient) : IMessagingService
{
    private readonly ServiceBusClient _serviceBusClient = serviceBusClient;

    public async Task SendMessageAsync<T>(T message, string queue, string? correlationId = null)
    {
        var sender = _serviceBusClient.CreateSender(queue);
        var jsonMessage = message is string ? message.ToString() : JsonSerializer.Serialize(message);
        ServiceBusMessage busMessage = new(jsonMessage);
        
        if (!string.IsNullOrEmpty(correlationId))
        {
            busMessage.CorrelationId = correlationId;
        }
        
        await sender.SendMessageAsync(busMessage);
    }
}
