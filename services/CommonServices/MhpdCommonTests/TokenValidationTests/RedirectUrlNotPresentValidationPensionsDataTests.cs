using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.TokenValidation;
using Microsoft.Extensions.Logging;
using Moq;

namespace MhpdCommonTests.TokenValidationTests;

public class RedirectUrlNotPresentValidationPensionsDataTests
{
    private readonly RedirectUrlNotPresentValidationPensionsData _notPresentValidationValidator;

    public RedirectUrlNotPresentValidationPensionsDataTests()
    {
        Mock<ILogger<RedirectUrlNotPresentValidationPensionsData>> loggerMock = new();
        _notPresentValidationValidator = new RedirectUrlNotPresentValidationPensionsData(loggerMock.Object);
    }

    [Fact]
    public void Validate_ShouldReturnFailure_WhenRedirectUriIsMissing()
    {
        var result = _notPresentValidationValidator.Validate(new PensionsDataRequestModel { RedirectUrl = string.Empty });

        Assert.False(result.IsValid);
        Assert.Equal(TokenValidationMessages.RedirectUriNotPresent, result.ErrorMessage);
    }

    [Fact]
    public void Validate_ShouldReturnSuccess_WhenRedirectUriIsProvided()
    {
        var result = _notPresentValidationValidator.Validate(new PensionsDataRequestModel { RedirectUrl = "https://www.example.com/api/1" });
        Assert.True(result.IsValid);
    }
}