using MhpdCommon.CustomExceptions;

namespace MhpdCommonTests.CustomExceptions;

public class ServiceCommunicationExceptionTests
{
    [Fact]
        public void Constructor_NoArguments_ShouldCreateExceptionWithDefaultMessage()
        {
            // Act
            var exception = new ServiceCommunicationException();

            // Assert
            Assert.NotNull(exception);
            Assert.Null(exception.InnerException); // Should not have an inner exception
            Assert.Equal("Exception of type 'MhpdCommon.CustomExceptions.ServiceCommunicationException' was thrown.", exception.Message);
        }

        [Fact]
        public void Constructor_WithMessage_ShouldSetMessage()
        {
            // Arrange
            var expectedMessage = "There was an error communicating with the service.";

            // Act
            var exception = new ServiceCommunicationException(expectedMessage);

            // Assert
            Assert.NotNull(exception);
            Assert.Equal(expectedMessage, exception.Message);
            Assert.Null(exception.InnerException); // Should not have an inner exception
        }

        [Fact]
        public void Constructor_WithMessageAndInnerException_ShouldSetMessageAndInnerException()
        {
            // Arrange
            var expectedMessage = "There was an error communicating with the service.";
            var innerException = new Exception("Inner exception message");

            // Act
            var exception = new ServiceCommunicationException(expectedMessage, innerException);

            // Assert
            Assert.NotNull(exception);
            Assert.Equal(expectedMessage, exception.Message);
            Assert.NotNull(exception.InnerException);
            Assert.Equal(innerException, exception.InnerException);
            Assert.Equal("Inner exception message", exception.InnerException?.Message);
        }

        [Fact]
        public void Constructor_WithInnerException_ShouldSetInnerException()
        {
            // Arrange
            var innerException = new Exception("Inner exception message");

            // Act
            var exception = new ServiceCommunicationException("Outer exception message", innerException);

            // Assert
            Assert.NotNull(exception);
            Assert.Equal("Outer exception message", exception.Message);
            Assert.NotNull(exception.InnerException);
            Assert.Equal(innerException, exception.InnerException);
        }
}