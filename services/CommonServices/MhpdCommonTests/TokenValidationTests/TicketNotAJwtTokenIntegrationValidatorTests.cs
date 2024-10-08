using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.TokenValidation;
using Microsoft.Extensions.Logging;
using Moq;

namespace MhpdCommonTests.TokenValidationTests;

public class TicketNotAJwtTokenIntegrationValidatorTests
{
    private readonly TicketNotAJwtTokenIntegrationValidator _validator;

    public TicketNotAJwtTokenIntegrationValidatorTests()
    {
        Mock<ILogger<TicketNotAJwtTokenIntegrationValidator>> loggerMock = new();
        _validator = new TicketNotAJwtTokenIntegrationValidator(loggerMock.Object);
    }

    [Fact]
    public void Validate_ShouldReturnFailure_WhenTicketIsNotJwt()
    {
        // Arrange
        var request = new TokenIntegrationRequestModel
        {
            Ticket = "invalid.jwt.token", // Invalid JWT format
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid); // Validation should fail
        Assert.Equal(TokenValidationMessages.InvalidTicketQueryFormat, result.ErrorMessage);
    }

    [Fact]
    public void Validate_ShouldReturnSuccess_WhenTicketIsValidJwt()
    {
        // Arrange
        var request = new TokenIntegrationRequestModel
        {
            Ticket = TokenQueryParams.ValidJwtToken,
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.True(result.IsValid); // Validation should succeed
    }
}