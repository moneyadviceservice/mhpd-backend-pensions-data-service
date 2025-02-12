namespace CryptopackProcessor.Models;

public class WebAppSettings
{
    public string AppName { get; set; } = string.Empty;

    public string SubscriptionId { get; set; } = string.Empty;

    public string ResourceGroupName { get; set; } = string.Empty;

    public string JwtKeyVariable { get; set; } = string.Empty;

    public string JwtKidVariable { get; set; } = string.Empty;
}
