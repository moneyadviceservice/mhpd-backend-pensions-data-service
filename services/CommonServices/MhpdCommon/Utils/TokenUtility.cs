using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using MhpdCommon.Constants.HttpClient;
using MhpdCommon.Models.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace MhpdCommon.Utils;

public partial class TokenUtility : ITokenUtility
{
    // Regular expression pattern for valid code_verifier
    // Use the GeneratedRegexAttribute to compile the regex pattern at compile-time
    [GeneratedRegex(@"^[a-zA-Z0-9\-\.\\_\~]{43,128}$")]
    private static partial Regex CodeVerifierPattern();
    
    private readonly JwtSettings _jwtSettings;
    private readonly int _expiryInSeconds;

    public TokenUtility(IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;

        // Safely try to convert the string to an int else set to a default
        _expiryInSeconds = int.TryParse(_jwtSettings.ExpiryInSeconds, out var expiry) ? expiry : 600;
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
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, subject),
            new(JwtRegisteredClaimNames.Iss, issuer),
            new(JwtRegisteredClaimNames.Aud, audience),
            new(JwtRegisteredClaimNames.Iat, iat.ToString(), ClaimValueTypes.Integer64),
            new(JwtRegisteredClaimNames.Exp, exp.ToString(), ClaimValueTypes.Integer64),
            new(JwtRegisteredClaimNames.Jti, jti.ToString()) // Random 36-character GUID for jti
        };

        if (!string.IsNullOrWhiteSpace(peisStartCode))
        {
            claims.Add(new(QueryParams.Cda.Token.PeisId, peisId)); // Custom claim for peis_id
        }
        
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
    
    public IDictionary<string, string> DecodeJwt(string token)
    {
        var jwtHandler = new JwtSecurityTokenHandler();
        var jwtToken = jwtHandler.ReadJwtToken(token);

        var claimsDictionary = new Dictionary<string, string>();
        
        foreach (var claim in jwtToken.Claims)
        {
            claimsDictionary[claim.Type] = claim.Value;
        }

        return claimsDictionary;
    }
    
    public bool DoesRegexMatch(string input, string pattern)
    {
        try
        {
            // Create a new Regex instance with the pattern and a timeout
            var regexWithTimeout = new Regex(pattern, RegexOptions.None, TimeSpan.FromMilliseconds(500));

            // Use the new Regex with timeout to check if the input matches
            return regexWithTimeout.IsMatch(input);
        }
        catch (RegexMatchTimeoutException)
        {
            // Handle the timeout exception if the matching takes too long
            return false;
        }
    }
    
    public static bool IsValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult) 
               && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
    
    /// <summary>
    /// Validates that the input code_verifier is between 43 and 128 characters long and matches the required pattern.
    /// </summary>
    /// <param name="codeVerifier">The code verifier to validate.</param>
    /// <returns>True if the code_verifier is valid; otherwise, false.</returns>
    public static bool IsValidCodeVerifier(string codeVerifier)
    {
        if (string.IsNullOrEmpty(codeVerifier))
        {
            return false;
        }

        // Check if the code verifier matches the length requirements and regex pattern
        return CodeVerifierPattern().IsMatch(codeVerifier);
    }

    public string? RetrieveClaim(string token, string requiredClaimName)
    {
        var handler = new JwtSecurityTokenHandler();
        try
        {
            var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

            if (jsonToken!.Claims.FirstOrDefault(claim => claim.Type == requiredClaimName) != null)
            {
                return jsonToken!.Claims.First(claim => claim.Type == requiredClaimName).Value;
            }
        }
        catch (Exception)
        {
            return null;
        }

        return null;
    }
}