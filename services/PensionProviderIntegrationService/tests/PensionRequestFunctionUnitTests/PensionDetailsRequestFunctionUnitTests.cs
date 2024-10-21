using Azure.Messaging.ServiceBus;
using MhpdCommon.Models.Configuration;
using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.Utils;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using PensionRequestFunction;
using PensionRequestFunction.Constants;
using PensionRequestFunction.Orchestration;
using PensionRequestFunction.Transformer;

namespace PensionRequestFunctionUnitTests
{
    public class PensionDetailsRequestFunctionUnitTests
    {
        private readonly PensionDetailsRequestFunction _function;
        private readonly Mock<ServiceBusMessageActions> _actionsMock;
        private readonly Mock<IMessageParser> _messageParser;
        private readonly Mock<IIdValidator> _idValidatorMock;
        private readonly Mock<IViewDataOrchestrator> _viewDataOrchestratorMock;

        public PensionDetailsRequestFunctionUnitTests()
        {
            _viewDataOrchestratorMock = new Mock<IViewDataOrchestrator>();
            _viewDataOrchestratorMock.Setup(mock => mock.GetPensionViewDataAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>())).ReturnsAsync(string.Empty);

            var logger = new Mock<ILogger<PensionDetailsRequestFunction>>();
            var holderNameId = Guid.NewGuid().ToString();
            var assetId = Guid.NewGuid().ToString();

            _idValidatorMock = new Mock<IIdValidator>();
            _idValidatorMock.Setup(mock => mock.IsValidGuid(It.IsAny<string>())).Returns(true);
            _idValidatorMock.Setup(mock => mock.TryExtractPei(assetId, out holderNameId, out assetId)).Returns(true);

            _messageParser = new Mock<IMessageParser>();
            _messageParser.Setup(mock => mock.ToPensionRequestPayload(It.IsAny<string>())).Returns(new PensionRequestPayload());

            var messaging = new Mock<IMessagingService>();
            messaging.Setup(m => m.SendMessageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Verifiable();

            var config = new CommonServiceBusConfiguration();
            var options = Options.Create(config);

            var transformer = new Mock<IVewDataToPensionArrangementTransformer>();
            transformer.Setup(m => m.Transform(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(string.Empty);

            _function = new PensionDetailsRequestFunction(logger.Object, 
                _idValidatorMock.Object,
                _messageParser.Object,
                messaging.Object,
                _viewDataOrchestratorMock.Object,
                transformer.Object,
                options);

            _actionsMock = new Mock<ServiceBusMessageActions>();
            _actionsMock.Setup(x => x.DeadLetterMessageAsync(It.IsAny<ServiceBusReceivedMessage>(),
                null, It.IsAny<string>(), null, It.IsAny<CancellationToken>())).Verifiable();
            _actionsMock.Setup(x => x.AbandonMessageAsync(It.IsAny<ServiceBusReceivedMessage>(),
                null, It.IsAny<CancellationToken>())).Verifiable();
            _actionsMock.Setup(x => x.CompleteMessageAsync(It.IsAny<ServiceBusReceivedMessage>(), It.IsAny<CancellationToken>())).Verifiable();
        }


        [Fact]
        public void WhenMssageReceivedIsValid_ThenMessageIsCompleted()
        {
            // Arrange
            var message = ServiceBusModelFactory.ServiceBusReceivedMessage(body: new BinaryData(ValidPayload));

            // Act
            var result = _function.Run(message, _actionsMock.Object);

            // Assert
            _actionsMock.Verify(r => r.CompleteMessageAsync(message, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public void WhenMessageReceivedWithValidationErrors_ThenMessageIsDLQed()
        {
            // Arrange
            var message = ServiceBusModelFactory.ServiceBusReceivedMessage(body: new BinaryData(ValidPayload));
            _messageParser.Setup(mock => mock.ToPensionRequestPayload(It.IsAny<string>())).Throws<AggregateException>();

            // Act
            var result = _function.Run(message, _actionsMock.Object);

            // Assert
            _actionsMock.Verify(r => r.DeadLetterMessageAsync(message, null,
            It.Is<string>(arg => arg.StartsWith(StatusConstants.PayloadInvalid)), null, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public void WhenMessageReceivedWithNoCorrelationId_ThenMessageIsDLQed()
        {
            var message = ServiceBusModelFactory.ServiceBusReceivedMessage(body: new BinaryData(ValidPayload));
            _messageParser.Setup(mock => mock.ToPensionRequestPayload(It.IsAny<string>())).Throws<AggregateException>();
            _idValidatorMock.Setup(mock => mock.IsValidGuid(null)).Returns(false);

            // Act
            var result = _function.Run(message, _actionsMock.Object);

            // Assert
            _actionsMock.Verify(r => r.DeadLetterMessageAsync(message, null,
            It.IsAny<string>(), null, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public void WhenMessageReceivedWithNoPayload_ThenMessageIsDLQed()
        {
            PensionRequestPayload? payload = null;
            var message = ServiceBusModelFactory.ServiceBusReceivedMessage(body: new BinaryData(ValidPayload));
            _messageParser.Setup(mock => mock.ToPensionRequestPayload(It.IsAny<string>())).Returns(payload);

            // Act
            var result = _function.Run(message, _actionsMock.Object);

            // Assert
            _actionsMock.Verify(r => r.DeadLetterMessageAsync(message, null,
            It.Is<string>(arg => arg.Equals(StatusConstants.PayloadInvalid)), null, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public void WhenViewDataOrchestrationFails_ThenMessageIsDLQed()
        {
            var message = ServiceBusModelFactory.ServiceBusReceivedMessage(body: new BinaryData(ValidPayload));
            _viewDataOrchestratorMock.Setup(mock => mock.GetPensionViewDataAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>())).Throws<FormatException>();

            // Act
            var result = _function.Run(message, _actionsMock.Object);

            // Assert
            _actionsMock.Verify(r => r.DeadLetterMessageAsync(message, null,
            It.IsAny<string>(), null, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public void WhenViewDataOrchestrationReturnsNull_ThenMessageIsDLQed()
        {
            string? viewDataContent = null;
            var message = ServiceBusModelFactory.ServiceBusReceivedMessage(body: new BinaryData(ValidPayload));
            _viewDataOrchestratorMock.Setup(mock => mock.GetPensionViewDataAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>())).ReturnsAsync(viewDataContent);

            // Act
            var result = _function.Run(message, _actionsMock.Object);

            // Assert
            _actionsMock.Verify(r => r.DeadLetterMessageAsync(message, null,
            It.IsAny<string>(), null, It.IsAny<CancellationToken>()), Times.Once);
        }

        private static string ValidPayload => """
            {
                "pensionRetrievalRecordId": "e01a9df7-f147-4a3a-a1dd-0507432a5b7f",
                "peI": "XXX7075aa11-10ad-4b2f-a9f5-1068e79119bf:1ba03e25-659a-43b8-ae77-b956df168969",
                "iss": "DATA_PROVIDER_1fd1da88-9fb3-461c-a48a-3dba21bfba17",
                "userSessionId": "459566f6-5fce-479e-a098-298ca9676a85"
            }
            """;
    }
}