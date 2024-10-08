using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.TokenValidation;
using Microsoft.Extensions.Logging;
using Moq;

namespace MhpdCommonTests.TokenValidationTests;

public class RqpNotAJwtValidatorTests
{
    private readonly RqpNotAJwtValidator _validator;

    public RqpNotAJwtValidatorTests()
    {
        Mock<ILogger<RqpNotAJwtValidator>> loggerMock = new();
        _validator = new RqpNotAJwtValidator(loggerMock.Object);
    }

    [Fact]
    public void Validate_ShouldReturnFailure_WhenRqpIsNotJwt()
    {
        // Arrange
        var request = new TokenIntegrationRequestModel
        {
            Rqp = "invalid.jwt.token", // Invalid JWT format
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid); // Validation should fail
        Assert.Equal(TokenValidationMessages.InvalidRqpFormat, result.ErrorMessage);
    }

    [Fact]
    public void Validate_ShouldReturnSuccess_WhenRqpIsValidJwt()
    {
        // Arrange
        var request = new TokenIntegrationRequestModel
        {
            Rqp = TokenQueryParams.ValidJwtToken,
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.True(result.IsValid); // Validation should succeed
    }
}