using Microsoft.Extensions.Options;

namespace CryptopackProcessor.Models;

public class SettingsContainer(IOptions<Manifest> manifestOptions, IOptions<CryptopackSettings> cryptoOptions, 
    IOptions<WebAppSettings> appSettings, IOptions<KeyVaultSettings> vaultOptions)
{
    public Manifest Manifest { get; } = manifestOptions.Value;

    public CryptopackSettings CryptopackSettings { get; } = cryptoOptions.Value;

    public WebAppSettings WebAppSettings { get; } = appSettings.Value;

    public AzureAdSettings AzureAdSettings { get; } = vaultOptions.Value;
}
