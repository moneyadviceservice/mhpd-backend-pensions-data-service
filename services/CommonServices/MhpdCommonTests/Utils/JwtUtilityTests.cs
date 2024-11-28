using System.IdentityModel.Tokens.Jwt;
using MhpdCommon.Models.MHPDModels.JwkUri;
using MhpdCommon.SharedHttpClient;
using MhpdCommon.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Moq;

namespace MhpdCommonTests.Utils;

public class JwtUtilityTests
{
    private readonly Mock<ISharedHttpClient> _sharedHttpClientMock;
    private readonly Mock<ILogger> _loggerMock;
    private readonly JwtUtility _jwtUtility;

    public JwtUtilityTests()
    {
        _sharedHttpClientMock = new Mock<ISharedHttpClient>();
        _loggerMock = new Mock<ILogger>();
        _jwtUtility = new JwtUtility(_sharedHttpClientMock.Object);
    }
    
    [Fact]
    public async Task ValidateJwtTokenAsync_MissingKid_ThrowsInvalidOperationException()
    {
        // Arrange: Generate an invalid JWT token with tampered or incorrect signature
        var header = new JwtHeader(
            new SigningCredentials(
                new RsaSecurityKey(
                    new System.Security.Cryptography.RSACryptoServiceProvider(2048)),
                SecurityAlgorithms.RsaSha256)
            );
        
        var payload = new JwtPayload
        {
            { "sub", "1234567890" },
            { "name", "John Doe" },
            { "iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds() }
        };
        
        var jwt = new JwtSecurityToken(header, payload);
        
        // The token's signature will be invalid because we're not signing it with a real private key.
        var invalidToken = new JwtSecurityTokenHandler().WriteToken(jwt);

        // Simulate the JWKS response with an incorrect public key (wrong modulus or exponent)
        var jwkResponse = new JwkUriResponseModel
        {
            Keys =
            [
                new JwkKey()
                {
                    KeyId = "validKid", // This should match the "kid" in the token header
                    KeyType = "RSA",
                    Modulus = "modulus", // Invalid modulus for the test
                    Exponent = "AQAB" // The public key doesn't match the private key that signed the JWT
                }
            ]
        };

        _sharedHttpClientMock.Setup(x => x.GetAsync()).ReturnsAsync(jwkResponse);

        // Act & Assert: Expect a SecurityTokenInvalidSignatureException because the token's signature is invalid
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _jwtUtility.ValidateJwtTokenWithKidAsync(invalidToken, _loggerMock.Object));

        Assert.Contains("The JWT does not contain a 'kid' header", exception.Message);
    }
    
        [Fact]
    public async Task ValidateJwtTokenAsync_InvalidKid_ThrowsInvalidOperationException()
    {
        // Arrange: Generate an invalid JWT token with tampered or incorrect signature
        var header = new JwtHeader(
            new SigningCredentials(
                new RsaSecurityKey(
                    new System.Security.Cryptography.RSACryptoServiceProvider(2048))
                {
                    KeyId = JwkConstants.KeyId // Set the kid
                },
                SecurityAlgorithms.RsaSha256)
        );
        
        var payload = new JwtPayload
        {
            { "sub", "1234567890" },
            { "name", "John Doe" },
            { "iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds() }
        };
        
        var jwt = new JwtSecurityToken(header, payload);
        
        // The token's signature will be invalid because we're not signing it with a real private key.
        var invalidToken = new JwtSecurityTokenHandler().WriteToken(jwt);

        // Simulate the JWKS response with an incorrect public key (wrong modulus or exponent)
        var jwkResponse = new JwkUriResponseModel
        {
            Keys =
            [
                new JwkKey()
                {
                    KeyId = "validKid", // This should match the "kid" in the token header
                    KeyType = "RSA",
                    Modulus = "modulus", // Invalid modulus for the test
                    Exponent = "AQAB" // The public key doesn't match the private key that signed the JWT
                }
            ]
        };

        _sharedHttpClientMock.Setup(x => x.GetAsync()).ReturnsAsync(jwkResponse);

        // Act & Assert: Expect a SecurityTokenInvalidSignatureException because the token's signature is invalid
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _jwtUtility.ValidateJwtTokenWithKidAsync(invalidToken, _loggerMock.Object));

        Assert.Contains($"Invalid 'kid': ${JwkConstants.KeyId}. No matching key found in the JWK set", exception.Message);
    }

    [Fact]
    public async Task ValidateJwtTokenAsync_InvalidIdToken_ThrowsInvalidOperationException()
    {
        // Arrange: Generate an invalid JWT token with tampered or incorrect signature
        var header = new JwtHeader(
            new SigningCredentials(
                new RsaSecurityKey(
                    new System.Security.Cryptography.RSACryptoServiceProvider(2048))
                {
                    KeyId = "validKid" // Set the kid
                },
                SecurityAlgorithms.RsaSha256)
        );
        
        var payload = new JwtPayload
        {
            { "sub", "1234567890" },
            { "name", "John Doe" },
            { "iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds() }
        };
        
        var jwt = new JwtSecurityToken(header, payload);
        
        // The token's signature will be invalid because we're not signing it with a real private key.
        var invalidToken = new JwtSecurityTokenHandler().WriteToken(jwt);

        // Simulate the JWKS response with an incorrect public key (wrong modulus or exponent)
        var jwkResponse = new JwkUriResponseModel
        {
            Keys = new List<JwkKey>
            {
                new JwkKey
                {
                    KeyId = "validKid", // This should match the "kid" in the token header
                    KeyType = "RSA",
                    Modulus = "modulus", // Invalid modulus for the test
                    Exponent = "AQAB" // The public key doesn't match the private key that signed the JWT
                }
            }
        };

        _sharedHttpClientMock.Setup(x => x.GetAsync()).ReturnsAsync(jwkResponse);

        // Act & Assert: Expect a SecurityTokenInvalidSignatureException because the token's signature is invalid
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _jwtUtility.ValidateJwtTokenWithKidAsync(invalidToken, _loggerMock.Object));

        Assert.Contains("Token validation failed", exception.Message);
    }
}