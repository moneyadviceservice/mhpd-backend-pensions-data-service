using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.TokenValidation;
using Microsoft.Extensions.Logging;
using Moq;

namespace MhpdCommonTests.TokenValidationTests;

public class CodeVerifierNotBase64StringPensionsDataTests
{
    private readonly CodeVerifierNotBase64StringPensionsData _notBase63StringValidator;

    public CodeVerifierNotBase64StringPensionsDataTests()
    {
        Mock<ILogger<CodeVerifierNotBase64StringPensionsData>> loggerMock = new();
        _notBase63StringValidator = new CodeVerifierNotBase64StringPensionsData(loggerMock.Object);
    }

    [Fact]
    public void Validate_ShouldReturnFailure_WhenCodeVerifierIsNotBase64()
    {
        var result = _notBase63StringValidator.Validate(new PensionsDataRequestModel { CodeVerifier = "test-string" });

        Assert.False(result.IsValid);
        Assert.Equal(TokenValidationMessages.InvalidCodeVerifier, result.ErrorMessage);
    }

    [Fact]
    public void Validate_ShouldReturnSuccess_WhenCodeVerifierIsBase64()
    {
        var result = _notBase63StringValidator.Validate(new PensionsDataRequestModel { CodeVerifier = "7189b64cc5f65b805baf201e384dc53ae7d18305d5ebb6170ad557b6" });
        Assert.True(result.IsValid);
    }
}