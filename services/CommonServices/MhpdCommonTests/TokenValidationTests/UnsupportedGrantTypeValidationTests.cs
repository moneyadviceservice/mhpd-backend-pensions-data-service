using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.TokenValidation;
using Microsoft.Extensions.Logging;
using Moq;

namespace MhpdCommonTests.TokenValidationTests;

public class UnsupportedGrantTypeValidationTests
{
    private readonly UnsupportedGrantTypeValidation _validation;

    public UnsupportedGrantTypeValidationTests()
    {
        Mock<ILogger<UnsupportedGrantTypeValidation>> loggerMock = new();
        _validation = new UnsupportedGrantTypeValidation(loggerMock.Object);
    }

    [Fact]
    public void Validate_ShouldReturnFailure_WhenGrantTypeIsEmpty()
    {
        var result = _validation.Validate(new CdaTokenRequestModel { GrantType = "" });

        Assert.False(result.IsValid);
        Assert.Equal(TokenValidationMessages.UnsupportedGrantType, result.ErrorMessage);
    }

    [Fact]
    public void Validate_ShouldReturnSuccess_WhenGrantTypeIsUma()
    {
        var result = _validation.Validate(new CdaTokenRequestModel { GrantType = TokenQueryParams.UmaGrantType });

        Assert.True(result.IsValid);
    }
    
    [Fact]
    public void Validate_ShouldReturnSuccess_WhenGrantTypeIsAuthorizationCode()
    {
        var result = _validation.Validate(new CdaTokenRequestModel { GrantType = TokenQueryParams.AuthorizationCodeGrantType });

        Assert.True(result.IsValid);
    }
    
    [Fact]
    public void Validate_ShouldReturnSuccess_WhenGrantTypeIsUnknown()
    {
        var result = _validation.Validate(new CdaTokenRequestModel { GrantType = "Unknown" });

        Assert.False(result.IsValid);
        Assert.Equal(TokenValidationMessages.UnsupportedGrantType, result.ErrorMessage);
    }
}
