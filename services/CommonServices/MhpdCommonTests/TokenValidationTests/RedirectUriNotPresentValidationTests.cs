using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.TokenValidation;
using Microsoft.Extensions.Logging;
using Moq;

namespace MhpdCommonTests.TokenValidationTests;

public class RedirectUriNotPresentValidationTests
{
    private readonly RedirectUriNotPresentValidation _notPresentValidationValidator;

    public RedirectUriNotPresentValidationTests()
    {
        Mock<ILogger<RedirectUriNotPresentValidation>> loggerMock = new();
        _notPresentValidationValidator = new RedirectUriNotPresentValidation(loggerMock.Object);
    }

    [Fact]
    public void Validate_ShouldReturnFailure_WhenRedirectUriIsMissing()
    {
        var result = _notPresentValidationValidator.Validate(new CdaTokenRequestModel { RedirectUri = string.Empty });

        Assert.False(result.IsValid);
        Assert.Equal(TokenValidationMessages.InvalidRequest, result.ErrorMessage);
    }

    [Fact]
    public void Validate_ShouldReturnSuccess_WhenRedirectUriIsProvided()
    {
        var result = _notPresentValidationValidator.Validate(new CdaTokenRequestModel { RedirectUri = "https://www.example.com/api/1" });
        Assert.True(result.IsValid);
    }
}