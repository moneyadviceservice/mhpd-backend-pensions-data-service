using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.TokenValidation;
using Microsoft.Extensions.Logging;
using Moq;

namespace MhpdCommonTests.TokenValidationTests;

public class AuthorisationCodeInvalidFormatValidationPensionsDataTests
{
    private readonly AuthorisationCodeInvalidFormatValidationPensionsData _invalidFormatValidator;

    public AuthorisationCodeInvalidFormatValidationPensionsDataTests()
    {
        Mock<ILogger<AuthorisationCodeInvalidFormatValidationPensionsData>> loggerMock = new();
        _invalidFormatValidator = new AuthorisationCodeInvalidFormatValidationPensionsData(loggerMock.Object);
    }

    [Fact]
    public void Validate_ShouldReturnFailure_WhenCodeIsInvalidFormat()
    {
        var result = _invalidFormatValidator.Validate(new PensionsDataRequestModel { AuthorisationCode = "Hello\x80" });

        Assert.False(result.IsValid);
        Assert.Equal(TokenValidationMessages.InvalidAuthorisationCodeFormat, result.ErrorMessage);
    }

    [Fact]
    public void Validate_ShouldReturnSuccess_WhenCodeIsValidFormat()
    {
        var result = _invalidFormatValidator.Validate(new PensionsDataRequestModel { AuthorisationCode = "123e4567-e89b-12d3-a456-426614174000" });
        Assert.True(result.IsValid);
    }
}
