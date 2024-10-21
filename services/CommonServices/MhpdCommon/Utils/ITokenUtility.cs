namespace MhpdCommon.Utils;

public interface ITokenUtility
{
    public string GenerateJwt(string? peisStartCode);

    public bool DoesRegexMatch(string input, string pattern);

    public IDictionary<string, string> DecodeJwt(string token);

    string? RetrieveClaim(string token, string requiredClaimName);
}