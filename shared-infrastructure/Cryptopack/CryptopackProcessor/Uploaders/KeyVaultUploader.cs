using Azure.Identity;
using Azure.Security.KeyVault.Certificates;
using Azure.Security.KeyVault.Secrets;
using CryptopackProcessor.Extensions;
using CryptopackProcessor.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CryptopackProcessor.Uploaders;

public class KeyVaultUploader : IKeyVaultUploader
{
    private readonly KeyVaultSettings _keyVaultSettings;
    private readonly CryptopackSettings _cryptopackSettings;
    private readonly CertificateClient _certificateClient;
    private readonly SecretClient _secretClient;
    private readonly ILogger<KeyVaultUploader> _logger;

    public KeyVaultUploader(ILogger<KeyVaultUploader> logger, IOptions<KeyVaultSettings> vaultOptions,
    IOptions<CryptopackSettings> cryptoOptions)
    {
        _keyVaultSettings = vaultOptions.Value;
        _cryptopackSettings = cryptoOptions.Value;
        _logger = logger;

        var clientSecretCredential = new ClientSecretCredential(_keyVaultSettings.TenantId,
            _keyVaultSettings.ClientId, _keyVaultSettings.ClientSecret);
        _certificateClient = new CertificateClient(new Uri(_keyVaultSettings.KeyVaultUrl), clientSecretCredential);
        _secretClient = new SecretClient(new Uri(_keyVaultSettings.KeyVaultUrl), clientSecretCredential);
    }
    

    public async Task UploadCertificateAsync(byte[] pfxBytes)
    {
        var importOptions = new ImportCertificateOptions(_cryptopackSettings.MtlsCertificateName, pfxBytes)
        {
            Password = _cryptopackSettings.PfxPassword
        };

        var certificateOperation = await _certificateClient.ImportCertificateAsync(importOptions);
        var uploadResponse = certificateOperation.GetRawResponse();

        if (uploadResponse.IsError)
        {
            _logger.LogError("Certificate {certificateName} was not uploaded to the vault. Reason - {reason}",
                _cryptopackSettings.MtlsCertificateName, uploadResponse.ReasonPhrase);
        }
        else
        {
            _logger.LogWarning("Certificate {certificateName} uploaded successfully", _cryptopackSettings.MtlsCertificateName);
        }
    }

    public async Task UploadSecretsAsync(string jwtPrivateKey, string kid)
    {
        await UploadSecretAsync(_cryptopackSettings.PrivateKeySecretName, jwtPrivateKey.Flat());
        await UploadSecretAsync(_cryptopackSettings.KidSecretName, kid);
    }

    private async Task UploadSecretAsync(string secretName, string secretValue)
    {
        KeyVaultSecret secret = new(secretName, secretValue);
        var secretOperation = await _secretClient.SetSecretAsync(secret);
        var uploadResponse = secretOperation.GetRawResponse();

        if (uploadResponse.IsError)
        {
            _logger.LogError("Secret {secret} was not uploaded to the vault. Reason - {reason}",
                secretName, uploadResponse.ReasonPhrase);
        }
        else
        {
            _logger.LogWarning("Secret '{secret}' uploaded successfully.", secretName);
        }
    }
}
