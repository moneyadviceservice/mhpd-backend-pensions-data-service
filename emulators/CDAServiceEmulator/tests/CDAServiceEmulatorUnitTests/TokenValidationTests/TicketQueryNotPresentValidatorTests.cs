
using CDAServiceEmulator.Models.Token;
using CDAServiceEmulator.TokenValidation;
using Microsoft.Extensions.Logging;
using Moq;

namespace CDAServiceEmulatorUnitTests.TokenValidationTests;

public class TicketQueryNotPresentValidatorTests
{
    private readonly TicketQueryNotPresentValidator _validator;

    public TicketQueryNotPresentValidatorTests()
    {
        Mock<ILogger<TicketQueryNotPresentValidator>> loggerMock = new();
        _validator = new TicketQueryNotPresentValidator(loggerMock.Object);
    }

    [Fact]
    public void Validate_ShouldReturnFailure_WhenTicketIsMissingAndGrantTypeIsUmaTicket()
    {
        var result = _validator.Validate(new CdaTokenRequestModel { GrantType = TokenQueryParams.UmaGrantType, Ticket = "" });

        Assert.False(result.IsValid);
        Assert.Equal(TokenValidationMessages.InvalidTicketQuery, result.ErrorMessage);
    }

    [Fact]
    public void Validate_ShouldReturnSuccess_WhenTicketIsProvided()
    {
        var result = _validator.Validate(new CdaTokenRequestModel { GrantType = TokenQueryParams.UmaGrantType, Ticket = TokenQueryParams.ValidJwtToken });

        Assert.True(result.IsValid);
    }
}
