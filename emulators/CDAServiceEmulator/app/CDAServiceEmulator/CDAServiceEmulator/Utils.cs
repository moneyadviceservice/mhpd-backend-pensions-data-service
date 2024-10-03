using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using CDAServiceEmulator.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace CDAServiceEmulator;

public class Utils
{
    private readonly JwtSettings _jwtSettings;
    
    private readonly int _expiryInSeconds;

    public Utils(IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;

        // Safely try to convert the string to an int else set to a default
        _expiryInSeconds = int.TryParse(_jwtSettings.ExpiryInSeconds, out var expiry) ? expiry : 600;
    }

    // Method to check if a string matches the pattern
    public static bool IsValidString(string input)
    {
        // Regex pattern to match printable ASCII characters (space to tilde)
        const string pattern = @"^[\x20-\x7E]+$";

        // Return true if input matches the pattern
        return Regex.IsMatch(input, pattern, RegexOptions.None, TimeSpan.FromMilliseconds(500));
    }
    
    public string GenerateJwt(string? peisStartCode)
    {
        // Step 1: Create GUID for peis_id and jti
        var random = Guid.NewGuid().ToString()[4..];

        var peisId = peisStartCode + random;
        var jti = Guid.NewGuid();

        // Step 2: Set values for claims
        const string subject = "cf668d47-ee58-4e33-bc05-feb7058de58d";
        const string issuer = "https://emulators.maps.org.uk/am/oauth2";
        const string audience = "https://pdp/ig/token";
        
        // Step 3: Get current UTC timestamp for iat
        var iat = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        // Step 4: Set expiration time (iat + 600 seconds)
        var exp = iat + _expiryInSeconds;

        // Step 5: Define JWT claims
        var claims = new[]
        {
            new Claim("peis_id", peisId),  // Custom claim for peis_id
            new Claim(JwtRegisteredClaimNames.Sub, subject),
            new Claim(JwtRegisteredClaimNames.Iss, issuer),
            new Claim(JwtRegisteredClaimNames.Aud, audience),
            new Claim(JwtRegisteredClaimNames.Iat, iat.ToString(), ClaimValueTypes.Integer64),
            new Claim(JwtRegisteredClaimNames.Exp, exp.ToString(), ClaimValueTypes.Integer64),
            new Claim(JwtRegisteredClaimNames.Jti, jti.ToString()) // Random 36-character GUID for jti
        };
        
        // Step 6: Define signing credentials (for example, using HS256 symmetric key)
        var rsa = RSA.Create();
        rsa.ImportFromPem(_jwtSettings.PrivateKey);

        var credentials = new SigningCredentials(
            new RsaSecurityKey(rsa),
            SecurityAlgorithms.RsaSha256Signature
        );
        
        // Step 7: Create the JWT
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTimeOffset.FromUnixTimeSeconds(exp).UtcDateTime, // Set expiration
            signingCredentials: credentials
        );

        // Step 8: Return the generated JWT
        var jwtHandler = new JwtSecurityTokenHandler();
        return jwtHandler.WriteToken(token);
    }
}