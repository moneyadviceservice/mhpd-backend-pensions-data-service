using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.TokenValidation;
using Microsoft.Extensions.Logging;
using Moq;

namespace MhpdCommonTests.TokenValidationTests;

public class AsUriNotPresentValidatorTests
{
    private readonly AsUriNotPresentValidator _notPresentValidationValidator;

    public AsUriNotPresentValidatorTests()
    {
        Mock<ILogger<AsUriNotPresentValidator>> loggerMock = new();
        _notPresentValidationValidator = new AsUriNotPresentValidator(loggerMock.Object);
    }

    [Fact]
    public void Validate_ShouldReturnFailure_WhenRedirectUriIsMissing()
    {
        var result = _notPresentValidationValidator.Validate(new TokenIntegrationRequestModel { AsUri = string.Empty });

        Assert.False(result.IsValid);
        Assert.Equal(TokenValidationMessages.InvalidAsUri, result.ErrorMessage);
    }

    [Fact]
    public void Validate_ShouldReturnSuccess_WhenRedirectUriIsProvided()
    {
        var result = _notPresentValidationValidator.Validate(new TokenIntegrationRequestModel { AsUri = "https://www.example.com/api/1" });
        Assert.True(result.IsValid);
    }
}