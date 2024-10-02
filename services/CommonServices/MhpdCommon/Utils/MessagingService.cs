using Azure.Messaging.ServiceBus;
using System.Text.Json;

namespace MhpdCommon.Utils;

public class MessagingService(ServiceBusClient serviceBusClient) : IMessagingService
{
    private readonly ServiceBusClient _serviceBusClient = serviceBusClient;

    public async Task SendMessageAsync<T>(T message, string queue)
    {
        var sender = _serviceBusClient.CreateSender(queue);
        var jsonMessage = JsonSerializer.Serialize(message);
        ServiceBusMessage busMessage = new(jsonMessage);
        await sender.SendMessageAsync(busMessage);
    }
}
