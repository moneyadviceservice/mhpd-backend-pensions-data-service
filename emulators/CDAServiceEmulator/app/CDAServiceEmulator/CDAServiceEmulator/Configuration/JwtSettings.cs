namespace CDAServiceEmulator.Configuration;

public class JwtSettings
{
    public string PrivateKey { get; set; } = string.Empty;
    public int ExpiryInSeconds { get; set; }
}