using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.TokenValidation;
using Microsoft.Extensions.Logging;
using Moq;

namespace MhpdCommonTests.TokenValidationTests;

public class AsUriNotAUrlValidatorTests
{
    private readonly AsUriNotAUrlValidator _validator;

    public AsUriNotAUrlValidatorTests()
    {
        Mock<ILogger<AsUriNotAUrlValidator>> loggerMock = new();
        _validator = new AsUriNotAUrlValidator(loggerMock.Object);
    }

    [Fact]
    public void Validate_ShouldReturnFailure_WhenRedirectUriIsInvalidUrl()
    {
        var result = _validator.Validate(new TokenIntegrationRequestModel { AsUri = "htps://ww.example.com/api/1" });

        Assert.False(result.IsValid);
        Assert.Equal(TokenValidationMessages.InvalidAsUri, result.ErrorMessage);
    }

    [Fact]
    public void Validate_ShouldReturnSuccess_WhenRedirectUriIsValidUrl()
    {
        var result = _validator.Validate(new TokenIntegrationRequestModel { AsUri = Helper.ValidRedirectUri });
        Assert.True(result.IsValid);
    }
}