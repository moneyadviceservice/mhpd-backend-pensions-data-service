namespace MhpdCommon.Models.Configuration;

public class JwtSettings
{
    public string PrivateKey { get; set; } = string.Empty;
    public string ExpiryInSeconds { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string Kid { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
