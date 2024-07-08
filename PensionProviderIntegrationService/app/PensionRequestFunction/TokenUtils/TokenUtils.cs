using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;

namespace PensionRequestFunction.TokenUtils
{
    [ExcludeFromCodeCoverage]
    public class TokenDecoder()
    {
        public string RetrieveClaim(string token, string requiredClaimName, out string claimValue)
        {
            claimValue = string.Empty;
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

            return jsonToken!.Claims.First(claim => claim.Type == requiredClaimName).Value;
        }
    }
}
