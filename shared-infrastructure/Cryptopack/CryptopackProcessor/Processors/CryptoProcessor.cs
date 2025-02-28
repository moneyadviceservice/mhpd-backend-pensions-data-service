using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.AppService;
using Azure.ResourceManager.AppService.Models;
using CryptopackProcessor.Extensions;
using CryptopackProcessor.Generators;
using CryptopackProcessor.Models;
using CryptopackProcessor.Uploaders;
using Microsoft.Extensions.Logging;
using System.IO.Compression;

namespace CryptopackProcessor.Processors;

public class CryptoProcessor(ILogger<CryptoProcessor> logger, SettingsContainer settingsContainer, 
    ICertificateGeneratorFactory generatorFactory, IKeyVaultUploader vaultUploader) : ICryptoProcessor
{
    private readonly Manifest _manifest = settingsContainer.Manifest;
    private readonly CryptopackSettings _cryptopackSettings = settingsContainer.CryptopackSettings;
    private readonly WebAppSettings _webAppSettings = settingsContainer.WebAppSettings;
    private readonly AzureAdSettings _azureAdSettings = settingsContainer.AzureAdSettings;

    public async Task ProcessAsync(ZipArchive archive)
    {
        try
        {
            var certificatePem = archive.GetFileContents(_manifest.MtlsCertificate);
            var certChainPem = archive.GetFileContents(_manifest.MtlsChain);
            var certPrivateKeyPem = archive.GetFileContents(_manifest.CertificatePair.PrivateKey);

            // Generate and upload the MTLS certificate
            var pfxGenerator = generatorFactory.GetGenerator(_manifest.CertificatePair.AlgorithmType);
            var pfxBytes = pfxGenerator.GeneratePfx(certificatePem, certPrivateKeyPem, certChainPem, _cryptopackSettings.PfxPassword);

            if (pfxBytes == null || pfxBytes.Length == 0)
            {
                throw new InvalidOperationException("Pfx certificate was not generated. Further processing will be aborted.");
            }

            logger.LogInformation("Pfx certificate has been generated");

            await vaultUploader.UploadCertificateAsync(pfxBytes);

            // Upload the secrets
            var jwtPrivateKeyPem = archive.GetFileContents(_manifest.JwtPair.PrivateKey);
            var kidPem = archive.GetFileContents(_manifest.KeyId);

            await vaultUploader.UploadSecretsAsync(jwtPrivateKeyPem, kidPem);

            // Update and restart the affected service
            //await UpdateApplicationSettingsAsync(archive);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Error processing crypto pack contents");
        }
    }

    public async Task UpdateApplicationSettingsAsync(ZipArchive archive)
    {
        var credential = new ClientSecretCredential(_azureAdSettings.TenantId, _azureAdSettings.ClientId, _azureAdSettings.ClientSecret);
        var armClient = new ArmClient(credential);

        var webAppResourceId = WebSiteResource.CreateResourceIdentifier(_webAppSettings.SubscriptionId, 
            _webAppSettings.ResourceGroupName, _webAppSettings.AppName);

        var webApp = armClient.GetWebSiteResource(webAppResourceId);

        // Update the app settings
        var configResource = await webApp.GetApplicationSettingsAsync();
        var config = configResource.Value;

        SetEnvironmentVariable(config, _webAppSettings.JwtKeyVariable, archive.GetFileContents(_manifest.JwtPair.PrivateKey));
        SetEnvironmentVariable(config, _webAppSettings.JwtKidVariable, archive.GetFileContents(_manifest.KeyId));

        // Restart the service
        var operation = await webApp.UpdateApplicationSettingsAsync(config);
        var response = operation.GetRawResponse();

        if (response.IsError)
        {
            logger.LogError("Failed to update and restart {app}", _webAppSettings.AppName);
        }
        else
        {
            logger.LogInformation("Successfully updated and restarted {app}", _webAppSettings.AppName);
        }
    }

    private void SetEnvironmentVariable(AppServiceConfigurationDictionary appServiceConfiguration,
        string variableName, string variableValue)
    {
        if (appServiceConfiguration.Properties.ContainsKey(variableName))
        {
            appServiceConfiguration.Properties[variableName] = variableValue;
        }
        else
        {
            appServiceConfiguration.Properties.Add(variableName, variableValue);
        }

        logger.LogInformation("Environment variable {variable} has been updated", variableName);
    }
}
