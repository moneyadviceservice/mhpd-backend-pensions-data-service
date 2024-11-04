using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.TokenValidation;
using Microsoft.Extensions.Logging;
using Moq;

namespace MhpdCommonTests.TokenValidationTests;

public class RedirectUrlNotValidUrlValidationPensionsDataTests
{
    private readonly RedirectUrlNotValidUrlValidationPensionsData _validator;

    public RedirectUrlNotValidUrlValidationPensionsDataTests()
    {
        Mock<ILogger<RedirectUrlNotValidUrlValidationPensionsData>> loggerMock = new();
        _validator = new RedirectUrlNotValidUrlValidationPensionsData(loggerMock.Object);
    }

    [Fact]
    public void Validate_ShouldReturnFailure_WhenRedirectUriIsInvalidUrl()
    {
        var result = _validator.Validate(new PensionsDataRequestModel { RedirectUrl = "htps://ww.example.com/api/1" });

        Assert.False(result.IsValid);
        Assert.Equal(TokenValidationMessages.InvalidRedirectUri, result.ErrorMessage);
    }

    [Fact]
    public void Validate_ShouldReturnSuccess_WhenRedirectUriIsValidUrl()
    {
        var result = _validator.Validate(new PensionsDataRequestModel { RedirectUrl = Helper.ValidRedirectUri });
        Assert.True(result.IsValid);
    }
}
