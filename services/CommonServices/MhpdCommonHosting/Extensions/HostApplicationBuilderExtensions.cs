using System.Security.Cryptography.X509Certificates;
using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Certificates;
using MhpdCommon.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace MhpdCommon.Extensions;

public static class HostBuilderExtensions
{
    public static async Task<X509Certificate2> ConfigureMtlsWithClientCertificateAsync(this WebApplicationBuilder builder)
    {
        var keyVaultEndpoint = new Uri(builder.Configuration[SecurityConstants.Vault.Uri] ?? string.Empty);
        builder.Configuration.AddAzureKeyVault(keyVaultEndpoint, GetVaultCredential(builder.Environment, builder.Configuration));

        // Load the certificate from the key vault
        var certificateClient = new CertificateClient(keyVaultEndpoint, GetVaultCredential(builder.Environment, builder.Configuration));
        var certificateName = builder.Configuration[SecurityConstants.Vault.MtlsCertificate];
        KeyVaultCertificateWithPolicy vaultCertificate = await certificateClient.GetCertificateAsync(certificateName);
        var certificate = new X509Certificate2(vaultCertificate.Cer);

        // Configure Kestrel to require client certificates
        builder.WebHost.ConfigureKestrel(options =>
        {
            options.ConfigureHttpsDefaults(httpsOptions =>
            {
                httpsOptions.ClientCertificateMode = ClientCertificateMode.RequireCertificate;
                httpsOptions.CheckCertificateRevocation = true;
                httpsOptions.ClientCertificateValidation = (cert, chain, policyErrors) =>
                {
                    // This allows us to return a 403 later
                    return true;
                };
            });
        });

        return certificate;
    }

    private static TokenCredential GetVaultCredential(IHostEnvironment environment, IConfiguration configuration)
    {
        if (environment.IsDevelopment())
        {
            // Load credentials from secrets file
            var clientId = configuration[SecurityConstants.Identity.ClientId];
            var clientSecret = configuration[SecurityConstants.Identity.ClientSecret];
            var tenantId = configuration[SecurityConstants.Identity.TenantId];
            return new ClientSecretCredential(tenantId, clientId, clientSecret);
        }
        else
        {
            // Use Managed Identity in Azure
            return new DefaultAzureCredential();
        }
    }
}