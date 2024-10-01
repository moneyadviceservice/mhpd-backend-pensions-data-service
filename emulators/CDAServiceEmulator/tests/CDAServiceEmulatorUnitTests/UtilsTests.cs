using System.IdentityModel.Tokens.Jwt;
using CDAServiceEmulator;
using CDAServiceEmulator.Configuration;
using Microsoft.Extensions.Options;
using Moq;

namespace CDAServiceEmulatorUnitTests;

public class UtilsTests
{
    private readonly Utils _utils;

    public UtilsTests()
    {
        var configuration = new JwtSettings
        {
            PrivateKey = Helper.GeneratedRsaPrivateKeyPem
        };

        Mock<IOptions<JwtSettings>> mockJwtSettingsOptions = new();
        mockJwtSettingsOptions.Setup(x => x.Value).Returns(configuration);

        _utils = new Utils(mockJwtSettingsOptions.Object);
    }
    
    [Theory]
    [InlineData("Hello, World!", true)]  // Valid string, all characters are within the printable ASCII range
    [InlineData("1234567890", true)]      // Valid string, all numeric characters
    [InlineData(" ", true)]               // Valid string, single space
    [InlineData("!@#$%^&*()", true)]      // Valid string, special characters
    [InlineData("ASCII: \x7E\x20", true)] // Valid string, exact boundaries (space and tilde)
    [InlineData("", false)]               // Invalid: empty string
    [InlineData("\x19Hello", false)]      // Invalid: control character
    [InlineData("Hello\x80", false)]      // Invalid: non-ASCII character (> 0x7E)
    [InlineData("Valid\x7EChars\x80Here", false)] // Mixed case with non-ASCII character

    // Test method
    public void TestIsValidString(string input, bool expected)
    {
        // Act
        var result = Utils.IsValidString(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GenerateJwt_ShouldReturnValidJwt_WithExpectedClaims()
    {
        // Arrange
        var peisStartCode = "TEST";
        
        // Act
        var jwtToken = _utils.GenerateJwt(peisStartCode);
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(jwtToken);

        // Assert - Check if the token is not null
        Assert.NotNull(token);

        // Assert - Check if the token contains the expected claims
        Assert.Equal("cf668d47-ee58-4e33-bc05-feb7058de58d", token.Subject);
        Assert.Equal("https://emulators.maps.org.uk/am/oauth2", token.Issuer);
        Assert.Equal("https://pdp/ig/token", token.Audiences.FirstOrDefault());

        var iatClaim = token.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Iat);
        var expClaim = token.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Exp);
        var jtiClaim = token.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti);
        var peisIdClaim = token.Claims.FirstOrDefault(c => c.Type == "peis_id");

        // Assert - Check if the claims exist and are correct
        Assert.NotNull(iatClaim);
        Assert.NotNull(expClaim);
        Assert.NotNull(jtiClaim);
        Assert.NotNull(peisIdClaim);

        // Validate that peis_id starts with peisStartCode
        Assert.StartsWith(peisStartCode, peisIdClaim.Value);
    }
}