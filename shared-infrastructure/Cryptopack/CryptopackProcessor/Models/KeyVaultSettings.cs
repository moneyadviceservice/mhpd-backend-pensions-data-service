namespace CryptopackProcessor.Models;

public class KeyVaultSettings : AzureAdSettings
{
    public string KeyVaultUrl { get; set; } = string.Empty;
}
