namespace MhpdCommon.Utils;

public interface IMessagingService
{
    Task SendMessageAsync<T>(T message, string queue, string? correlationId = null);
}
