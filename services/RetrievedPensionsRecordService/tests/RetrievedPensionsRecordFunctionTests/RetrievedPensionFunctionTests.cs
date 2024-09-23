using Azure.Messaging.ServiceBus;
using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.Utils;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Moq;
using RetrievedPensionsRecordFunction;
using RetrievedPensionsRecordFunction.Repository;
using RetrievedPensionsRecordFunction.Utils;
using RetrievedPensionsRecordFunctionTests.Data;

namespace RetrievedPensionsRecordFunctionTests;

public class RetrievedPensionFunctionTests
{
    private readonly Mock<ILogger<RetrievedPensionsFunction>> _loggerMock;
    private readonly Mock<IIdValidator> _idValidatorMock;
    private readonly Mock<IPensionRecordValidator> _recordValidatorMock;
    private readonly Mock<IPensionRecordRepository> _repositoryMock;
    private readonly Mock<ServiceBusMessageActions> _actionsMock;
    private readonly Mock<IMessageParser> _messageParseMock;
    private readonly RetrievedPensionsFunction _function;
    private const string ValidateFailReason = "Bad Data";

    public RetrievedPensionFunctionTests()
    {
        _loggerMock = new Mock<ILogger<RetrievedPensionsFunction>>();

        _idValidatorMock = new Mock<IIdValidator>();
        _idValidatorMock.Setup(x => x.IsValidGuid(It.IsAny<string>())).Returns(false);
        _idValidatorMock.Setup(x => x.IsValidPeI(It.IsAny<string>())).Returns(false);

        var reason = ValidateFailReason;
        _recordValidatorMock = new Mock<IPensionRecordValidator>();
        _recordValidatorMock.Setup(x => x.ValidateRecord(It.IsAny<RetrievedPensionDetailsPayload>(), out reason)).Returns(false);

        _repositoryMock = new Mock<IPensionRecordRepository>();
        _repositoryMock.Setup(x => x.SaveRetrievedPensionRecordAsync(It.IsAny<string>(), It.IsAny<RetrievedPensionDetailsPayload>())).ReturnsAsync(true);

        _messageParseMock = new Mock<IMessageParser>();
        var error = new AggregateException(new Exception("Bad Data"));
        _messageParseMock.Setup(x => x.ToRetrievedPensionPayload(It.IsAny<string>())).Throws(error);

        _function = new RetrievedPensionsFunction(_loggerMock.Object, _idValidatorMock.Object, _messageParseMock.Object, _recordValidatorMock.Object, _repositoryMock.Object);

        _actionsMock = new Mock<ServiceBusMessageActions>();
        _actionsMock.Setup(x => x.DeadLetterMessageAsync(It.IsAny<ServiceBusReceivedMessage>(),
            null, It.IsAny<string>(), null, It.IsAny<CancellationToken>())).Verifiable();
        _actionsMock.Setup(x => x.AbandonMessageAsync(It.IsAny<ServiceBusReceivedMessage>(),
            null, It.IsAny<CancellationToken>())).Verifiable();
        _actionsMock.Setup(x => x.CompleteMessageAsync(It.IsAny<ServiceBusReceivedMessage>(), It.IsAny<CancellationToken>())).Verifiable();
    }

    [Fact]
    public async Task Run_ShouldCallDeadLetterQueue_OnNoCorrelationId()
    {
        ResetInvocations();

        //arrange
        _actionsMock.Invocations.Clear();
        _loggerMock.Invocations.Clear();
        var message = ServiceBusModelFactory.ServiceBusReceivedMessage(
            body: new BinaryData("Test message"));

        // Act
        await _function.Run(message, _actionsMock.Object);

        // Assert
        var reason = "Missing or Invalid correlationId:";
        _actionsMock.Verify(r => r.DeadLetterMessageAsync(message, null,
            It.Is<string>(arg => arg.StartsWith(reason)), null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Run_ShouldCallDeadLetterQueue_OnPayloadParseFail()
    {
        ResetInvocations();

        //arrange
        _idValidatorMock.Setup(x => x.IsValidGuid(It.IsAny<string>())).Returns(true);

        var content = DataProvider.GetString("InvalidRecordIdPayload.json");
        var message = ServiceBusModelFactory.ServiceBusReceivedMessage(
            body: BinaryData.FromString(content), correlationId: Guid.NewGuid().ToString());

        // Act
        await _function.Run(message, _actionsMock.Object);

        // Assert
        _actionsMock.Verify(r => r.DeadLetterMessageAsync(message, null,
            It.Is<string>(arg => arg.StartsWith("Invalid retrieved pension payload")), null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Run_ShouldCallDeadLetterQueue_OnPayloadValidateFail()
    {
        ResetInvocations();

        //arrange
        const string file = "EmptyGuidRecordIdPayload.json";
        var payload = DataProvider.GetPayload(file);
        _idValidatorMock.Setup(x => x.IsValidGuid(It.IsAny<string>())).Returns(true);
        _messageParseMock.Setup(x => x.ToRetrievedPensionPayload(It.IsAny<string>())).Returns(payload);

        var content = DataProvider.GetString(file);
        var message = ServiceBusModelFactory.ServiceBusReceivedMessage(
            body: BinaryData.FromString(content), correlationId: Guid.NewGuid().ToString());

        // Act
        await _function.Run(message, _actionsMock.Object);

        // Assert
        _actionsMock.Verify(r => r.DeadLetterMessageAsync(message, null,
            It.Is<string>(arg => arg.StartsWith(ValidateFailReason)), null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Run_ShouldCallAbandonMessage_OnSaveFail()
    {
        ResetInvocations();

        //arrange
        var reason = string.Empty;
        const string file = "ValidRetrievedPensionPayload.json";
        var payload = DataProvider.GetPayload(file);

        _idValidatorMock.Setup(x => x.IsValidGuid(It.IsAny<string>())).Returns(true);
        _messageParseMock.Setup(x => x.ToRetrievedPensionPayload(It.IsAny<string>())).Returns(payload);
        _recordValidatorMock.Setup(x => x.ValidateRecord(It.IsAny<RetrievedPensionDetailsPayload>(), out reason)).Returns(true);
        _repositoryMock.Setup(x => x.SaveRetrievedPensionRecordAsync(It.IsAny<string>(), It.IsAny<RetrievedPensionDetailsPayload>())).ReturnsAsync(false);

        var content = DataProvider.GetString(file);
        var message = ServiceBusModelFactory.ServiceBusReceivedMessage(
            body: BinaryData.FromString(content), correlationId: Guid.NewGuid().ToString());

        // Act
        await _function.Run(message, _actionsMock.Object);

        // Assert
        _actionsMock.Verify(r => r.AbandonMessageAsync(message, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Run_ShouldCallCompleteMessage_OnSaveSuccess()
    {
        ResetInvocations();

        //arrange
        var reason = string.Empty;
        const string file = "ValidRetrievedPensionPayload.json";
        var payload = DataProvider.GetPayload(file);
        
        _idValidatorMock.Setup(x => x.IsValidGuid(It.IsAny<string>())).Returns(true);
        _messageParseMock.Setup(x => x.ToRetrievedPensionPayload(It.IsAny<string>())).Returns(payload);
        _recordValidatorMock.Setup(x => x.ValidateRecord(It.IsAny<RetrievedPensionDetailsPayload>(), out reason)).Returns(true);
        _repositoryMock.Setup(x => x.SaveRetrievedPensionRecordAsync(It.IsAny<string>(), It.IsAny<RetrievedPensionDetailsPayload>())).ReturnsAsync(true);

        var content = DataProvider.GetString(file);
        var message = ServiceBusModelFactory.ServiceBusReceivedMessage(
            body: BinaryData.FromString(content), correlationId: Guid.NewGuid().ToString());

        // Act
        await _function.Run(message, _actionsMock.Object);

        // Assert
        _actionsMock.Verify(r => r.CompleteMessageAsync(message, It.IsAny<CancellationToken>()), Times.Once);
    }

    private void ResetInvocations()
    {
        _actionsMock.Invocations.Clear();
    }
}
