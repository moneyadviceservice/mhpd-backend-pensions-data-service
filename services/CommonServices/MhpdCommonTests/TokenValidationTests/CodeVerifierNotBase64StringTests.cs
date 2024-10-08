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
    
    [Fact]
    public void IsValidCodeVerifier_ValidCodeVerifier_ReturnsTrue()
    {
        // Arrange
        const string validCodeVerifier = "7189b64cc5f65b805baf201e384dc53ae7d18305d5ebb6170ad557b6"; // 60 characters

        // Act
        var result = CodeVerifierNotBase64String.IsValidCodeVerifier(validCodeVerifier);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValidCodeVerifier_TooShortCodeVerifier_ReturnsFalse()
    {
        // Arrange
        const string shortCodeVerifier = "short123"; // Less than 43 characters

        // Act
        var result = CodeVerifierNotBase64String.IsValidCodeVerifier(shortCodeVerifier);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValidCodeVerifier_TooLongCodeVerifier_ReturnsFalse()
    {
        // Arrange
        var longCodeVerifier = new string('a', 129); // More than 128 characters

        // Act
        var result = CodeVerifierNotBase64String.IsValidCodeVerifier(longCodeVerifier);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValidCodeVerifier_InvalidCharacters_ReturnsFalse()
    {
        // Arrange
        const string invalidCodeVerifier = "7189b64cc5f65b805baf201e384dc53ae7d18305d5ebb6170ad557b6@#"; // Invalid special characters

        // Act
        var result = CodeVerifierNotBase64String.IsValidCodeVerifier(invalidCodeVerifier);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValidCodeVerifier_NullOrEmptyCodeVerifier_ReturnsFalse()
    {
        // Arrange
        string nullCodeVerifier = null!;
        const string emptyCodeVerifier = "";

        // Act & Assert
        Assert.False(CodeVerifierNotBase64String.IsValidCodeVerifier(nullCodeVerifier));
        Assert.False(CodeVerifierNotBase64String.IsValidCodeVerifier(emptyCodeVerifier));
    }

    [Fact]
    public void IsValidCodeVerifier_BoundaryLengthValid_ReturnsTrue()
    {
        // Arrange
        var minLengthCodeVerifier = new string('a', 43); // Exactly 43 characters
        var maxLengthCodeVerifier = new string('a', 128); // Exactly 128 characters

        // Act & Assert
        Assert.True(CodeVerifierNotBase64String.IsValidCodeVerifier(minLengthCodeVerifier));
        Assert.True(CodeVerifierNotBase64String.IsValidCodeVerifier(maxLengthCodeVerifier));
    }
}