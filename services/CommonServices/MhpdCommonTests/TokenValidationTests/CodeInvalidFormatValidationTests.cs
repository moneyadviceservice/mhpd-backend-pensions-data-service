using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.TokenValidation;
using Microsoft.Extensions.Logging;
using Moq;

namespace MhpdCommonTests.TokenValidationTests;

public class CodeInvalidFormatValidationTests
{
    private readonly CodeInvalidFormatValidation _invalidFormatValidator;

    public CodeInvalidFormatValidationTests()
    {
        Mock<ILogger<CodeInvalidFormatValidation>> loggerMock = new();
        _invalidFormatValidator = new CodeInvalidFormatValidation(loggerMock.Object);
    }

    [Fact]
    public void Validate_ShouldReturnFailure_WhenCodeIsInvalidFormat()
    {
        var result = _invalidFormatValidator.Validate(new CdaTokenRequestModel { Code = "Hello\x80" });

        Assert.False(result.IsValid);
        Assert.Equal(TokenValidationMessages.InvalidCodeFormat, result.ErrorMessage);
    }

    [Fact]
    public void Validate_ShouldReturnSuccess_WhenCodeIsValidFormat()
    {
        var result = _invalidFormatValidator.Validate(new CdaTokenRequestModel { Code = "123e4567-e89b-12d3-a456-426614174000" });
        Assert.True(result.IsValid);
    }
}
