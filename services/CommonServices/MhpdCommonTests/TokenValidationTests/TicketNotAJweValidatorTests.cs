using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.TokenValidation;
using Microsoft.Extensions.Logging;
using Moq;

namespace MhpdCommonTests.TokenValidationTests;

public class TicketNotAJweValidatorTests
{
    private readonly TicketNotAJweValidator _validator;

    public TicketNotAJweValidatorTests()
    {
        Mock<ILogger<TicketNotAJweValidator>> loggerMock = new();
        _validator = new TicketNotAJweValidator(loggerMock.Object);
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
        Assert.Equal(TokenValidationMessages.InvalidJweTicketQueryFormat, result.ErrorMessage);
    }

    [Fact]
    public void Validate_ShouldReturnSuccess_WhenTicketIsValidJwt()
    {
        // Arrange
        var request = new CdaTokenRequestModel
        {
            Ticket = TokenQueryParams.ValidJweToken,
            GrantType = TokenQueryParams.UmaGrantType
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.True(result.IsValid); // Validation should succeed
        Assert.Equal(10, _validator.Order);
    }
}