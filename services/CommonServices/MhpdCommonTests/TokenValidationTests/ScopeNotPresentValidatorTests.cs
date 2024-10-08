using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.TokenValidation;
using Microsoft.Extensions.Logging;
using Moq;

namespace MhpdCommonTests.TokenValidationTests;

public class ScopeNotPresentValidatorTests
{
    private readonly ScopeNotPresentValidator _validator;

    public ScopeNotPresentValidatorTests()
    {
        Mock<ILogger<ScopeNotPresentValidator>> loggerMock = new();
        _validator = new ScopeNotPresentValidator(loggerMock.Object);
    }

    [Fact]
    public void Validate_ShouldReturnFailure_WhenScopeIsMissingAndGrantTypeIsUmaTicket()
    {
        var result = _validator.Validate(new CdaTokenRequestModel { GrantType = TokenQueryParams.UmaGrantType, Scope = "" });

        Assert.False(result.IsValid);
        Assert.Equal(TokenValidationMessages.InvalidScope, result.ErrorMessage);
    }

    [Fact]
    public void Validate_ShouldReturnSuccess_WhenScopeIsProvided()
    {
        var result = _validator.Validate(new CdaTokenRequestModel { GrantType = TokenQueryParams.UmaGrantType, Scope = TokenQueryParams.Owner });

        Assert.True(result.IsValid);
        Assert.Equal(3, _validator.Order);
    }
}
