using System.IdentityModel.Tokens.Jwt;
using MhpdCommon.SharedHttpClient;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace MhpdCommon.Utils;

public class JwtUtility(ISharedHttpClient sharedHttpClient) : IJwtUtility
{
    public async Task ValidateJwtTokenWithKidAsync(string idToken, ILogger logger)
    {
        // Decode the JWT
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(idToken);
        var header = jwt.Header;

        // Extract the 'kid' from the header
        if (!header.TryGetValue("kid", out var kid))
        {
            logger.LogError("The JWT does not contain a 'kid' header");
            throw new InvalidOperationException("The JWT does not contain a 'kid' header");
        }

        // Fetch JWKs from URI
        var jwkUriResponse = await sharedHttpClient.GetAsync();
        if (!jwkUriResponse.Keys.Exists(k => k.KeyId == (string)kid))
        {
            logger.LogError("Invalid 'kid': {Kid}. No matching key found in the JWK set", kid);
            throw new InvalidOperationException($"Invalid 'kid': ${kid}. No matching key found in the JWK set");
        }

        var jwks = new JsonWebKeySet(System.Text.Json.JsonSerializer.Serialize(jwkUriResponse));

        // Token validation parameters
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            TryAllIssuerSigningKeys = false, // Only match the key with the specific 'kid'
            IssuerSigningKeys = jwks.Keys
        };

        try
        {
            // Validate the JWT
            handler.ValidateToken(idToken, tokenValidationParameters, out _);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Token validation failed");
            throw new InvalidOperationException("Token validation failed");
        }
    }
}