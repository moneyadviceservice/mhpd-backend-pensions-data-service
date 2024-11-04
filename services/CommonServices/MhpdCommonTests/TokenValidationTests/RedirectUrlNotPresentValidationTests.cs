using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.TokenValidation;
using Microsoft.Extensions.Logging;
using Moq;

namespace MhpdCommonTests.TokenValidationTests;

public class RedirectUrlNotPresentValidationTests
{
    private readonly RedirectUrlNotPresentValidation _notPresentValidationValidator;

    public RedirectUrlNotPresentValidationTests()
    {
        Mock<ILogger<RedirectUrlNotPresentValidation>> loggerMock = new();
        _notPresentValidationValidator = new RedirectUrlNotPresentValidation(loggerMock.Object);
    }

    [Fact]
    public void Validate_ShouldReturnFailure_WhenRedirectUriIsMissing()
    {
        var result = _notPresentValidationValidator.Validate(new CdaTokenRequestModel { RedirectUrl = string.Empty });

        Assert.False(result.IsValid);
        Assert.Equal(TokenValidationMessages.InvalidRequest, result.ErrorMessage);
    }

    [Fact]
    public void Validate_ShouldReturnSuccess_WhenRedirectUriIsProvided()
    {
        var result = _notPresentValidationValidator.Validate(new CdaTokenRequestModel { RedirectUrl = "https://www.example.com/api/1" });
        Assert.True(result.IsValid);
    }
}