using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.TokenValidation;
using Microsoft.Extensions.Logging;
using Moq;

namespace MhpdCommonTests.TokenValidationTests;

public class RedirectUriNotPresentValidationPensionsDataTests
{
    private readonly RedirectUriNotPresentValidationPensionsData _notPresentValidationValidator;

    public RedirectUriNotPresentValidationPensionsDataTests()
    {
        Mock<ILogger<RedirectUriNotPresentValidationPensionsData>> loggerMock = new();
        _notPresentValidationValidator = new RedirectUriNotPresentValidationPensionsData(loggerMock.Object);
    }

    [Fact]
    public void Validate_ShouldReturnFailure_WhenRedirectUriIsMissing()
    {
        var result = _notPresentValidationValidator.Validate(new PensionsDataRequestModel { RedirectUri = string.Empty });

        Assert.False(result.IsValid);
        Assert.Equal(TokenValidationMessages.RedirectUriNotPresent, result.ErrorMessage);
    }

    [Fact]
    public void Validate_ShouldReturnSuccess_WhenRedirectUriIsProvided()
    {
        var result = _notPresentValidationValidator.Validate(new PensionsDataRequestModel { RedirectUri = "https://www.example.com/api/1" });
        Assert.True(result.IsValid);
    }
}