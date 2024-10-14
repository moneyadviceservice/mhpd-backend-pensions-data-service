using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.TokenValidation;
using Microsoft.Extensions.Logging;
using Moq;

namespace MhpdCommonTests.TokenValidationTests;

public class CodeVerifierNotBase64StringTests
{
    private readonly CodeVerifierNotBase64String _notBase63StringValidator;

    public CodeVerifierNotBase64StringTests()
    {
        Mock<ILogger<CodeVerifierNotBase64String>> loggerMock = new();
        _notBase63StringValidator = new CodeVerifierNotBase64String(loggerMock.Object);
    }

    [Fact]
    public void Validate_ShouldReturnFailure_WhenCodeVerifierIsNotBase64()
    {
        var result = _notBase63StringValidator.Validate(new CdaTokenRequestModel { CodeVerifier = "test-string" });

        Assert.False(result.IsValid);
        Assert.Equal(TokenValidationMessages.InvalidCodeVerifier, result.ErrorMessage);
    }

    [Fact]
    public void Validate_ShouldReturnSuccess_WhenCodeVerifierIsBase64()
    {
        var result = _notBase63StringValidator.Validate(new CdaTokenRequestModel { CodeVerifier = "7189b64cc5f65b805baf201e384dc53ae7d18305d5ebb6170ad557b6" });
        Assert.True(result.IsValid);
    }
}