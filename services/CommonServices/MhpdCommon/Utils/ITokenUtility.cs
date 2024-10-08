namespace MhpdCommon.Utils;

public interface ITokenUtility
{
    public string GenerateJwt(string? peisStartCode);

    public bool DoesRegexMatch(string input, string pattern);
}