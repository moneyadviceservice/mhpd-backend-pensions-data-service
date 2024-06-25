using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NSubstitute;
using PensionRequestFunction;

namespace PensionRequestFunctionUnitTests
{
    public class PensionDetailsRequestFunctionUnitTests
    {

        private readonly Mock<ILoggerFactory> _mockLoggerFactory;
        private readonly Mock<ILogger<PensionDetailsRequestFunction>> _mockLogger;
        private readonly PensionDetailsRequestFunction _function;
        private readonly IConfiguration _configuration;

        public PensionDetailsRequestFunctionUnitTests()
        {
            _mockLoggerFactory = new Mock<ILoggerFactory>();
            _mockLogger = new Mock<ILogger<PensionDetailsRequestFunction>>();
            _mockLoggerFactory.Setup(factory => factory.CreateLogger(It.IsAny<string>())).Returns(_mockLogger.Object);

            _configuration = new ConfigurationBuilder()
                .AddJsonFile("local.settings.json")
                .Build();

            _function = new PensionDetailsRequestFunction(_mockLogger.Object, _configuration);
        }


        [Fact(Skip = "will be done ticket 24538 - Pension Provider Integration Service - respond (hard coded RPT)")]
        public void Test1()
        {
            // Arrange
            var receivedMessage = ServiceBusModelFactory.ServiceBusReceivedMessage(
                new BinaryData("Valid content"), "123", null, null, null, null, new TimeSpan(), null, null, null, "text/plain");

            var messageBody = """
            {
                "itemId": 1000,
                "category": "Books"
            }
            """;

            var message = ServiceBusModelFactory.ServiceBusReceivedMessage(body: new BinaryData(messageBody));

            var messageActions = Substitute.For<ServiceBusMessageActions>();

            // Act
            var result = _function.Run(message, messageActions);

            // Assert
            Assert.NotNull(result);
        }
    }
}