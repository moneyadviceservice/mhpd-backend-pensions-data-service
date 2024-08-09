using System.IdentityModel.Tokens.Jwt;

namespace PensionRequestFunction.TokenUtils
{
    public class TokenDecoder()
    {
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
            catch (Exception e)
            {
                return null;
            }

            return null;
        }
    }
}
