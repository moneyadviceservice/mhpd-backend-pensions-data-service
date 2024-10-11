using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using MaPSCDAService.Models;
using Microsoft.IdentityModel.Tokens;

namespace MaPSCDAService.Utils;

public class RqpTokenManager : IRqpTokenManager
{
    private const string Role = "owner";
    private readonly string? _kid;
    private readonly string? _audience;
    private readonly string? _privateKey;

    public RqpTokenManager(IConfiguration config)
    {
        _privateKey = config["privateKey"];
        _kid = config["Kid"];
        _audience = config["Audience"];
    }

    public string GenerateToken(string userSessionId, string iss)
    {
        var rsa = RSA.Create();
        rsa.ImportFromPem(_privateKey);

        var mySecurityKey = new SigningCredentials(
            new RsaSecurityKey(rsa) { KeyId = _kid }, 
            SecurityAlgorithms.RsaSha256Signature
        );

        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] 
            { 
                new Claim(JwtRegisteredClaimNames.Sub, $"{userSessionId}@{iss}"),
                new Claim(JwtRegisteredClaimNames.Iss, iss),
				new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(DateTime.UtcNow).ToString()),
                new Claim(JwtRegisteredClaimNames.Exp, new DateTimeOffset(DateTime.UtcNow.AddSeconds(60)).ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("Role", Role)

			}),
            IssuedAt = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddSeconds(60),
            Issuer = iss,
            Audience = _audience,
            SigningCredentials = mySecurityKey,
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }

    public bool ValidateToken(string token, string iss, out RQPModel rqpModel)
    {
        var rsa = RSA.Create();
        rsa.ImportFromPem(_privateKey);
        var mySecurityKey = new RsaSecurityKey(rsa) { KeyId = _kid };
        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = iss,
                ValidAudience = _audience,
                IssuerSigningKey = mySecurityKey,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);
    
            var jwtToken = (JwtSecurityToken)validatedToken;
    
            rqpModel = new RQPModel
            {
                Issuer = (jwtToken.Claims.First(x => x.Type == "iss").Value),
                IssuedAt = (jwtToken.Claims.First(x => x.Type == "iat").Value),
                Expiry = (jwtToken.Claims.First(x => x.Type == "exp").Value),
                Jti = (jwtToken.Claims.First(x => x.Type == "jti").Value),
                Subject = (jwtToken.Claims.First(x => x.Type == "sub").Value),
                Role = jwtToken.Claims.First(x => x.Type == "Role").Value,
                Audience = _audience,
            };
            
        }
        catch 
        {
            rqpModel = null!;
            return false;
        }
        
        return true;
    }
}