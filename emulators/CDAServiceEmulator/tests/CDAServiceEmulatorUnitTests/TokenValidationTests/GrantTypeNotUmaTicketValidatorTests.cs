using CDAServiceEmulator.Models.Token;
using CDAServiceEmulator.TokenValidation;
using Microsoft.Extensions.Logging;
using Moq;

namespace CDAServiceEmulatorUnitTests.TokenValidationTests;

public class GrantTypeNotUmaTicketValidatorTests
{
    private readonly GrantTypeNotUmaTicketValidator _notUmaTicketValidator;

    public GrantTypeNotUmaTicketValidatorTests()
    {
        Mock<ILogger<GrantTypeNotUmaTicketValidator>> loggerMock = new();
        _notUmaTicketValidator = new GrantTypeNotUmaTicketValidator(loggerMock.Object);
    }

    [Fact]
    public void Validate_ShouldReturnFailure_WhenGrantTypeIsNotUmaTicket()
    {
        var result = _notUmaTicketValidator.Validate(new CdaTokenRequestModel { GrantType = "" });

        Assert.False(result.IsValid);
        Assert.Equal(TokenValidationMessages.InvalidGrantType, result.ErrorMessage);
    }

    [Fact]
    public void Validate_ShouldReturnSuccess_WhenGrantTypeIsProvided()
    {
        var result = _notUmaTicketValidator.Validate(new CdaTokenRequestModel { GrantType = TokenQueryParams.UmaGrantType });

        Assert.True(result.IsValid);
        Assert.Equal(2, _notUmaTicketValidator.Order);
    }
}
