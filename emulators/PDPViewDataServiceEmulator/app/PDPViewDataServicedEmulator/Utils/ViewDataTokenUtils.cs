using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using PDPViewDataServicedEmulator.Mocks;
using PDPViewDataServicedEmulator.Models;

namespace PDPViewDataServicedEmulator.Utils
{
    public static class ViewDataTokenUtils
    {
        public class ViewDataTokenManager(
            string kid,
            string audience,
            string subject,
            ViewDataPayload viewData,
            string issuer)
        {
            private readonly string? _viewData = viewData.ViewData?.ToString();

            public string GenerateToken()
            {
                var rsa = RSA.Create();
                rsa.ImportFromPem(LoadGeneratedRsaPrivateKeyPem());

                var mySecurityKey = new SigningCredentials(
                    new RsaSecurityKey(rsa) { KeyId = kid },
                    SecurityAlgorithms.RsaSha256Signature
                );

                var tokenHandler = new JwtSecurityTokenHandler();

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                        new Claim(JwtRegisteredClaimNames.Sub, subject),
                        new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(DateTime.UtcNow).ToString()),
                        new Claim(JwtRegisteredClaimNames.Exp, new DateTimeOffset(DateTime.UtcNow.AddSeconds(60)).ToString()),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                        new Claim(JwtRegisteredClaimNames.Aud, audience),
                        new Claim(JwtRegisteredClaimNames.Iss, issuer),
                        new Claim("view_data", _viewData ?? string.Empty),
                    }),

                    IssuedAt = DateTime.UtcNow,
                    Expires = DateTime.UtcNow.AddSeconds(60),

                    SigningCredentials = mySecurityKey,
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                return tokenHandler.WriteToken(token);
            }

            public bool ValidateToken(string token, out TokenModel tokenModel)
            {
                var rsa = RSA.Create();
                rsa.ImportFromPem(LoadGeneratedRsaPrivateKeyPem());
                var mySecurityKey = new RsaSecurityKey(rsa) { KeyId = kid };

                var tokenHandler = new JwtSecurityTokenHandler();
                try
                {
                    tokenHandler.ValidateToken(token, new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidIssuer = issuer,
                        ValidAudience = audience,
                        IssuerSigningKey = mySecurityKey,
                        ClockSkew = TimeSpan.Zero
                    }, out SecurityToken validatedToken);

                    var jwtToken = (JwtSecurityToken)validatedToken;

                    tokenModel = new TokenModel
                    {
                        Issuer = jwtToken.Claims.First(x => x.Type == "iss").Value,
                        IssuedAt = jwtToken.Claims.First(x => x.Type == "iat").Value,
                        Expiry = jwtToken.Claims.First(x => x.Type == "exp").Value,
                        Jti = jwtToken.Claims.First(x => x.Type == "jti").Value,
                        Subject = jwtToken.Claims.First(x => x.Type == "sub").Value,
                        Audience = audience,
                    };

                }
                catch
                {
                    tokenModel = null!;
                    return false;
                }

                return true;
            }

            private static string LoadGeneratedRsaPrivateKeyPem()
            {
                return "-----BEGIN RSA PRIVATE KEY-----\r\nMIIEpAIBAAKCAQEAu+NIp4+Hm5qJoEmQwfYm/+JTph1bqTow6bwTWGVLb0LihRQH\r\n2Z5im04zBY7JekJcY4t3xiCz4lbO5aAjPkWRSNMLCQtT3U/tE4ouQc3CXal/BKVV\r\nThzEx4uRYOlfwm7MBuP0ywmArqL/uzv+nXR6d1SbJen0aPVq5GI1+pZPD32XQrx1\r\nVSPRvwHdbJKlJKv0Am0l5JAJhlqasWcL39qXVdlUl7BSpQ0RsqI9vQDTHaHT4f0a\r\n9WaE/0Bqs0wU5lwGKCfrF3QQyPTfqiRBKJhS8OJWA5Umm9OmZyUUvfhX9TR2yi0a\r\nB91wZixUZ1moc0c5hDbgT3ApwpqMfmpmD/MB4QIDAQABAoIBAQCbDCpurBEaJWMh\r\nNNQSW9E/JEQnNt7nPbATkjLcpWqkvCs16pu3x+5Tfyq3kWdOTujy9Kq4g1AunbhK\r\n4eVzg/EqiY33vrNMVKKtl0Ao6WVV6YN6D/3fYfo5OUGVGcD+MHSJ0x+1VXgtpTEo\r\nD5BD21UcmGMX3ILnAm9dXHZy/grsGijjO6kUXjFUFfiw01hNncOebAKRver7FaK7\r\noeac0QO1Pld2b/IBw5SkaqnQ9+cU4+RA6R23GKhvj+aMMCVNQ+vNr5wZd1Po7+HB\r\n6QAckfCtkSHtaGGt2vg/Ry19qRjLFq/Z9P87r6gvAgE/b3nCWJEmsqcXjtarMUus\r\nHVZkvnSJAoGBAPnpqm7F5SQGide98GeLrw71JqIO/UC+vsNTO6juMo0/cLu1k4Ju\r\n6wHJjW8EYK23V15zPQU+zrpc4ns6ZftQcVDfpXZDHMfWxZCGsmLH7gFjCXGxzC+t\r\nFtXklx1Skf23nZ9KK2SMsvoPckBplWSw+rNf5e4a1QOlOI+PkHWtzG1fAoGBAMB2\r\n3F1VvFFnz/o2trja0mGvd7hlla2E6PrpLAdzUdRXcBns+WA3i6zdmdj+p7Na5ba+\r\nDL8ZAt3hPIQa+1sjfdmtOTNvYIJEooLUcC70REa1BeQ0DCDomYyNzeAR1pU4Qv8l\r\nLN0iteKLben9BllRASijhGQwPfaNSLwzIf2go5i/AoGAaY9bYALS8M6aNriR7QrB\r\nXM4MDXPLYSewqAxmLMrAK41abs8ZiYSUW2LpDLWKqJXCn7CJW8fVxj4po+dc4DRw\r\nSLrOxw89/uMm/A8JFlRgodFVUeLJ3nF8LciqU8ZmyAZg28GWZphPmPQhTDfM1IU0\r\nv8OH+XT3+Nw28dywJiTNLn0CgYAeYKRXdvjKQIBrFcexVZhvIqrax+3f/WJod/Uj\r\nF+iKg2KGNQkHTr0VA8UMouUFQguP1F9eqivxqWNL1pZlxCxQ9y5YF/Q7m2mrDKzI\r\nPHWqU1gitLRVXWEe9iLQgwBDfSXV76AtJxVeNHOcwvsFjeuI5oY26gZdq98XFVsA\r\nFSK9MQKBgQD5dsOKU+9iDVe9vPEArP61jI2YWlXHQ//pUnpP6nOvnLRwfiZ3uCWy\r\nUnPQm5hpHkQ3yWAZ7Qg9VmHdfeW37+s7vrecjopPfEAzkRBylqb6f8PvBpvbxBr3\r\nkIlKA7uY0Jma1Cr1Cz4q/BepAeVmXrgK9k6hJJQsPkTM/s4LpjZQ/g==\r\n-----END RSA PRIVATE KEY-----";
            }
        }
    }
}
