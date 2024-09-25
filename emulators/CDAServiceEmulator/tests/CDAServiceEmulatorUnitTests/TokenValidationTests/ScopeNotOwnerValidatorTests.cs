using CDAServiceEmulator.Models.Token;
using CDAServiceEmulator.TokenValidation;
using Microsoft.Extensions.Logging;
using Moq;

namespace CDAServiceEmulatorUnitTests.TokenValidationTests;

public class ScopeNotOwnerValidatorTests
{
    private readonly ScopeNotOwnerValidator _validator;

    public ScopeNotOwnerValidatorTests()
    {
        Mock<ILogger<ScopeNotOwnerValidator>> loggerMock = new();
        _validator = new ScopeNotOwnerValidator(loggerMock.Object);
    }

    [Fact]
    public void Validate_ShouldReturnFailure_WhenScopeIsNotOwner()
    {
        var result = _validator.Validate(new CdaTokenRequestModel { GrantType = TokenQueryParams.UmaGrantType, Scope = "not_owner" });

        Assert.False(result.IsValid);
        Assert.Equal(TokenValidationMessages.ScopeNotOwner, result.ErrorMessage);
    }

    [Fact]
    public void Validate_ShouldReturnSuccess_WhenScopeIsOwner()
    {
        var result = _validator.Validate(new CdaTokenRequestModel { GrantType = TokenQueryParams.UmaGrantType, Scope = TokenQueryParams.Owner });

        Assert.True(result.IsValid);
    }
}
