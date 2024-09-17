using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Moq;
using PensionsRetrievalFunction;

namespace PensionsRetrievalFunctionTests;

public class RetrievalFunctionTests
{
    private readonly Mock<ServiceBusMessageActions> _actionsMock;

    public RetrievalFunctionTests()
    {
        _actionsMock = new Mock<ServiceBusMessageActions>();
        _actionsMock.Setup(x => x.DeadLetterMessageAsync(It.IsAny<ServiceBusReceivedMessage>(),
            null, It.IsAny<string>(), null, It.IsAny<CancellationToken>())).Verifiable();
        _actionsMock.Setup(x => x.AbandonMessageAsync(It.IsAny<ServiceBusReceivedMessage>(),
            null, It.IsAny<CancellationToken>())).Verifiable();
        _actionsMock.Setup(x => x.CompleteMessageAsync(It.IsAny<ServiceBusReceivedMessage>(), It.IsAny<CancellationToken>())).Verifiable();
    }

    [Fact]
    public async Task WhenFunctionIsInvoked_MessageIsProcessed()
    {
        //Arrange
        var loggerMock = new Mock<ILogger<RetrievalFunction>>();
        var function = new RetrievalFunction(loggerMock.Object);
        var message = ServiceBusModelFactory.ServiceBusReceivedMessage(
        body: new BinaryData("Test message"));

        //Act
        await function.Run(message, _actionsMock.Object);

        // Assert
        _actionsMock.Verify(r => r.CompleteMessageAsync(message, It.IsAny<CancellationToken>()), Times.Once);
    }
}