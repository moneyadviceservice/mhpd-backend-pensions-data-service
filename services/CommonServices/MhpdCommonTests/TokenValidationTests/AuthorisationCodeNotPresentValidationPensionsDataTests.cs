using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.TokenValidation;
using Microsoft.Extensions.Logging;
using Moq;

namespace MhpdCommonTests.TokenValidationTests;

public class AuthorisationCodeNotPresentValidationPensionsDataTests
{
    private readonly AuthorisationCodeNotPresentValidationPensionsData _notPresentValidationValidator;

    public AuthorisationCodeNotPresentValidationPensionsDataTests()
    {
        Mock<ILogger<AuthorisationCodeNotPresentValidationPensionsData>> loggerMock = new();
        _notPresentValidationValidator = new AuthorisationCodeNotPresentValidationPensionsData(loggerMock.Object);
    }

    [Fact]
    public void Validate_ShouldReturnFailure_WhenCodeIsMissing()
    {
        var result = _notPresentValidationValidator.Validate(new PensionsDataRequestModel { AuthorisationCode = string.Empty });

        Assert.False(result.IsValid);
        Assert.Equal(TokenValidationMessages.MissingAuthorisationCode, result.ErrorMessage);
    }

    [Fact]
    public void Validate_ShouldReturnSuccess_WhenCodeIsProvided()
    {
        var result = _notPresentValidationValidator.Validate(new PensionsDataRequestModel { AuthorisationCode = "123e4567-e89b-12d3-a456-426614174000" });
        Assert.True(result.IsValid);
    }
}
