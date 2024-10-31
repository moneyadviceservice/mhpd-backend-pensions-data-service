using MhpdCommon.Extensions;
using Microsoft.Extensions.Logging;
using Moq;

namespace MhpdCommonTests.Extensions;

public class LoggerExtensionsTests
{
    [Fact]
    public void BeginCorrelationScope_SetsCorrelationIdInScope()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var correlationId = Guid.NewGuid().ToString();
        var source = "My Mock Service";
        IDisposable disposableMock = Mock.Of<IDisposable>();

        loggerMock
            .Setup(logger => logger.BeginScope(It.IsAny<Dictionary<string, object>>()))
            .Returns(disposableMock)
            .Verifiable();

        // Act
        using var scope = loggerMock.Object.BeginCorrelationScope(correlationId, source);

        // Assert
        loggerMock.Verify(logger => logger.BeginScope(
            It.Is<Dictionary<string, object>>(v => v.ContainsKey(ILoggerExtensions.CorrelationId) && v[ILoggerExtensions.CorrelationId].Equals(correlationId))),
            Times.Once);

        loggerMock.Verify(logger => logger.BeginScope(
            It.Is<Dictionary<string, object>>(v => v.ContainsKey(ILoggerExtensions.Source) && v[ILoggerExtensions.Source].Equals(source))),
            Times.Once);
    }
}
