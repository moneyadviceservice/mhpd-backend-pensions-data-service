using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.TokenValidation;
using Microsoft.Extensions.Logging;
using Moq;

namespace MhpdCommonTests.TokenValidationTests;

public class RqpNotPresentValidatorTests
{
    private readonly RqpNotPresentValidator _notPresentValidationValidator;

    public RqpNotPresentValidatorTests()
    {
        Mock<ILogger<RqpNotPresentValidator>> loggerMock = new();
        _notPresentValidationValidator = new RqpNotPresentValidator(loggerMock.Object);
    }

    [Fact]
    public void Validate_ShouldReturnFailure_WhenRedirectUriIsMissing()
    {
        var result = _notPresentValidationValidator.Validate(new TokenIntegrationRequestModel { Rqp = string.Empty });

        Assert.False(result.IsValid);
        Assert.Equal(TokenValidationMessages.InvalidRqp, result.ErrorMessage);
    }

    [Fact]
    public void Validate_ShouldReturnSuccess_WhenRedirectUriIsProvided()
    {
        var result = _notPresentValidationValidator.Validate(new TokenIntegrationRequestModel { Rqp = TokenQueryParams.ValidJwtToken });
        Assert.True(result.IsValid);
    }
}