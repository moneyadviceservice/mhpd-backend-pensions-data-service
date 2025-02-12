using CryptopackProcessor.Extensions;
using CryptopackProcessor.Models;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace CryptopackProcessor.Generators;

public class PfxGenerator(ILogger<PfxGenerator> logger) : IPfxGenerator
{
    public byte[]? GeneratePfx(string certPem, string privateKeyPem, string? certChainPem, string password)
    {
        var sanitizedPem = certPem.Sanitized();

        var certBytes = Convert.FromBase64String(sanitizedPem);
        var certificate = new X509Certificate2(certBytes);

        try
        {
            var privateKey = ECDsa.Create();
            privateKey.ImportFromPem(privateKeyPem);

            var certWithPrivateKey = certificate.CopyWithPrivateKey(privateKey);

            var collection = new X509Certificate2Collection
            {
                certWithPrivateKey
            };

            if (!string.IsNullOrWhiteSpace(certChainPem))
            {
                // Split the certificate chain PEM string into individual certificate blocks
                var chainCerts = certChainPem.Split(new[] { Constants.SecurityKey.CertificateEnd }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var chainCert in chainCerts)
                {
                    var chainCertPem = chainCert.Sanitized();

                    if (!string.IsNullOrWhiteSpace(chainCert))
                    {
                        var chainCertBytes = Convert.FromBase64String(chainCertPem);
                        var chain = new X509Certificate2(chainCertBytes);
                        collection.Add(chain);
                    }
                }
            }

            // Export the collection to a PFX byte array
            return collection.Export(X509ContentType.Pkcs12, password);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unable to create a PFX file from the provided certificate, chain and private key");
            return null;
        }
    }
}
