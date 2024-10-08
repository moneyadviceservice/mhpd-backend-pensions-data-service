namespace MhpdCommon.Models.Configuration;

public class JwtSettings
{
    public string PrivateKey { get; set; } = string.Empty;
    public string ExpiryInSeconds { get; set; } = string.Empty;
}