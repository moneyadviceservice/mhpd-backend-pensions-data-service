using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.TokenValidation;
using Microsoft.Extensions.Logging;
using Moq;

namespace MhpdCommonTests.TokenValidationTests;

public class RedirectUrlNotValidUrlValidationTests
{
    private readonly RedirectUrlNotValidUrlValidation _validator;

    public RedirectUrlNotValidUrlValidationTests()
    {
        Mock<ILogger<RedirectUrlNotValidUrlValidation>> loggerMock = new();
        _validator = new RedirectUrlNotValidUrlValidation(loggerMock.Object);
    }

    [Fact]
    public void Validate_ShouldReturnFailure_WhenRedirectUriIsInvalidUrl()
    {
        var result = _validator.Validate(new CdaTokenRequestModel { RedirectUrl = "htps://ww.example.com/api/1" });

        Assert.False(result.IsValid);
        Assert.Equal(TokenValidationMessages.InvalidRedirectUri, result.ErrorMessage);
    }

    [Fact]
    public void Validate_ShouldReturnSuccess_WhenRedirectUriIsValidUrl()
    {
        var result = _validator.Validate(new CdaTokenRequestModel { RedirectUrl = Helper.ValidRedirectUri });
        Assert.True(result.IsValid);
    }
}
