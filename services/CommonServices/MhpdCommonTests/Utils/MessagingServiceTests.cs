using Azure.Messaging.ServiceBus;
using MhpdCommon.Utils;
using Moq;

namespace MhpdCommonTests.Utils;

public class MessagingServiceTests
{
    private readonly Mock<ServiceBusClient> _mockServiceBusClient;
    private readonly Mock<ServiceBusSender> _mockServiceBusSender;

    public MessagingServiceTests()
    {
        _mockServiceBusClient = new Mock<ServiceBusClient>();
        _mockServiceBusSender = new Mock<ServiceBusSender>();

        _mockServiceBusClient
            .Setup(client => client.CreateSender(It.IsAny<string>()))
            .Returns(_mockServiceBusSender.Object);
    }

    [Fact]
    public async Task SendMessageAsync_ShouldSendMessage()
    {
        // Arrange
        const string queue = "my-queue";
        var messagingService = new MessagingService(_mockServiceBusClient.Object);
        var testMessage = new TestPayload { TextProperty = "Test", NumericProperty = 123 };

        // Act
        await messagingService.SendMessageAsync(testMessage, queue);

        // Assert
        _mockServiceBusClient.Verify(sender => sender.CreateSender(queue), Times.Once);
        _mockServiceBusSender.Verify(sender => sender.SendMessageAsync(It.IsAny<ServiceBusMessage>(), default), Times.Once);
    }
}

public class TestPayload
{
    public string? TextProperty { get; set; }
    public int NumericProperty { get; set; }
}
