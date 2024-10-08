using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.TokenValidation;
using Microsoft.Extensions.Logging;
using Moq;

namespace MhpdCommonTests.TokenValidationTests;

public class RedirectUriNotValidUrlValidationTests
{
    private readonly RedirectUriNotValidUrlValidation _validator;

    public RedirectUriNotValidUrlValidationTests()
    {
        Mock<ILogger<RedirectUriNotValidUrlValidation>> loggerMock = new();
        _validator = new RedirectUriNotValidUrlValidation(loggerMock.Object);
    }

    [Fact]
    public void Validate_ShouldReturnFailure_WhenRedirectUriIsInvalidUrl()
    {
        var result = _validator.Validate(new CdaTokenRequestModel { RedirectUri = "htps://ww.example.com/api/1" });

        Assert.False(result.IsValid);
        Assert.Equal(TokenValidationMessages.InvalidRedirectUri, result.ErrorMessage);
    }

    [Fact]
    public void Validate_ShouldReturnSuccess_WhenRedirectUriIsValidUrl()
    {
        var result = _validator.Validate(new CdaTokenRequestModel { RedirectUri = Helper.ValidRedirectUri });
        Assert.True(result.IsValid);
    }
}
