using CDAServiceEmulator.Models.Token;
using CDAServiceEmulator.TokenValidation;
using Microsoft.Extensions.Logging;
using Moq;

namespace CDAServiceEmulatorUnitTests.TokenValidationTests;

public class RedirectUriNotValidUrlValidationTests
{
    private readonly RedirectUriNotValidUrlValidation _validator;

    public RedirectUriNotValidUrlValidationTests()
    {
        Mock<ILogger<RedirectUriNotValidUrlValidation>> loggerMock = new();
        _validator = new RedirectUriNotValidUrlValidation(loggerMock.Object);
    }

    [Fact]
    public void Validate_ShouldReturnFailure_WhenRedirectUriIsInvalidUrl()
    {
        var result = _validator.Validate(new CdaTokenRequestModel { RedirectUri = "htps://ww.example.com/api/1" });

        Assert.False(result.IsValid);
        Assert.Equal(TokenValidationMessages.InvalidRedirectUri, result.ErrorMessage);
    }

    [Fact]
    public void Validate_ShouldReturnSuccess_WhenRedirectUriIsValidUrl()
    {
        var result = _validator.Validate(new CdaTokenRequestModel { RedirectUri = Helper.ValidRedirectUri });
        Assert.True(result.IsValid);
    }
    
    [Fact]
    public void IsValidUrl_ValidHttpUrl_ReturnsTrue()
    {
        // Arrange
        const string url = "http://www.example.com";

        // Act
        var result = RedirectUriNotValidUrlValidation.IsValidUrl(url);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValidUrl_ValidHttpsUrl_ReturnsTrue()
    {
        // Arrange
        const string url = "https://www.example.com";

        // Act
        var result = RedirectUriNotValidUrlValidation.IsValidUrl(url);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValidUrl_InvalidUrlMissingScheme_ReturnsFalse()
    {
        // Arrange
        const string url = "www.example.com";

        // Act
        var result = RedirectUriNotValidUrlValidation.IsValidUrl(url);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValidUrl_InvalidUrlWithUnsupportedScheme_ReturnsFalse()
    {
        // Arrange
        const string url = "ftp://www.example.com";

        // Act
        var result = RedirectUriNotValidUrlValidation.IsValidUrl(url);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValidUrl_InvalidUrlMalformed_ReturnsFalse()
    {
        // Arrange
        const string url = "htp://example";

        // Act
        var result = RedirectUriNotValidUrlValidation.IsValidUrl(url);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValidUrl_EmptyString_ReturnsFalse()
    {
        // Arrange
        const string url = "";

        // Act
        var result = RedirectUriNotValidUrlValidation.IsValidUrl(url);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValidUrl_NullString_ReturnsFalse()
    {
        // Arrange
        string url = null!;

        // Act
        var result = RedirectUriNotValidUrlValidation.IsValidUrl(url);

        // Assert
        Assert.False(result);
    }
}
