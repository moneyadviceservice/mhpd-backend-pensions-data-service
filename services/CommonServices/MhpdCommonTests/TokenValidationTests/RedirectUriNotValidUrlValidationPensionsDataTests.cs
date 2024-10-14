using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.TokenValidation;
using Microsoft.Extensions.Logging;
using Moq;

namespace MhpdCommonTests.TokenValidationTests;

public class RedirectUriNotValidUrlValidationPensionsDataTests
{
    private readonly RedirectUriNotValidUrlValidationPensionsData _validator;

    public RedirectUriNotValidUrlValidationPensionsDataTests()
    {
        Mock<ILogger<RedirectUriNotValidUrlValidationPensionsData>> loggerMock = new();
        _validator = new RedirectUriNotValidUrlValidationPensionsData(loggerMock.Object);
    }

    [Fact]
    public void Validate_ShouldReturnFailure_WhenRedirectUriIsInvalidUrl()
    {
        var result = _validator.Validate(new PensionsDataRequestModel { RedirectUri = "htps://ww.example.com/api/1" });

        Assert.False(result.IsValid);
        Assert.Equal(TokenValidationMessages.InvalidRedirectUri, result.ErrorMessage);
    }

    [Fact]
    public void Validate_ShouldReturnSuccess_WhenRedirectUriIsValidUrl()
    {
        var result = _validator.Validate(new PensionsDataRequestModel { RedirectUri = Helper.ValidRedirectUri });
        Assert.True(result.IsValid);
    }
}
