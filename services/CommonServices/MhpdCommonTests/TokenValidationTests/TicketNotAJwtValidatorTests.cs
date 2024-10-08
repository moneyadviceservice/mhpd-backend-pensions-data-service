using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.TokenValidation;
using Microsoft.Extensions.Logging;
using Moq;

namespace MhpdCommonTests.TokenValidationTests;

public class TicketNotAJwtValidatorTests
{
    private readonly TicketNotAJwtValidator _validator;

    public TicketNotAJwtValidatorTests()
    {
        Mock<ILogger<TicketNotAJwtValidator>> loggerMock = new();
        _validator = new TicketNotAJwtValidator(loggerMock.Object);
    }

    [Fact]
    public void Validate_ShouldReturnFailure_WhenTicketIsNotJwt()
    {
        // Arrange
        var request = new CdaTokenRequestModel
        {
            Ticket = "invalid.jwt.token", // Invalid JWT format
            GrantType = TokenQueryParams.UmaGrantType
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
        var request = new CdaTokenRequestModel
        {
            Ticket = TokenQueryParams.ValidJwtToken,
            GrantType = TokenQueryParams.UmaGrantType
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.True(result.IsValid); // Validation should succeed
        Assert.Equal(10, _validator.Order);
    }
}