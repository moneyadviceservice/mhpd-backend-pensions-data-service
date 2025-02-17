using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;

namespace CryptopackProcessor.Generators;

public class EllipticalPfxGenerator(ILogger<PfxGenerator> logger) : IPfxGenerator
{
    public byte[]? GeneratePfx(string certPem, string privateKeyPem, string? certChainPem, string password)
    {
        try
        {
            // Parse the main certificate from PEM.
            X509Certificate bcCert;
            using (var reader = new StringReader(certPem))
            {
                var pemReader = new PemReader(reader);
                var obj = pemReader.ReadObject();
                if (obj is X509Certificate cert)
                {
                    bcCert = cert;
                }
                else
                {
                    throw new Exception("Unable to parse the certificate.");
                }
            }

            // Parse the EC private key from PEM.
            AsymmetricKeyParameter bcPrivateKey;
            using (var reader = new StringReader(privateKeyPem))
            {
                var pemReader = new PemReader(reader);
                var obj = pemReader.ReadObject();
                if (obj is AsymmetricCipherKeyPair keyPair)
                {
                    bcPrivateKey = keyPair.Private;
                }
                else if (obj is AsymmetricKeyParameter keyParam)
                {
                    bcPrivateKey = keyParam;
                }
                else
                {
                    throw new Exception("Unable to parse the private key.");
                }
            }

            // Parse the certificate chain (if provided).
            var chainCertificates = new List<X509Certificate>();
            if (!string.IsNullOrWhiteSpace(certChainPem))
            {
                // The chain PEM may contain multiple certificate blocks.
                using var reader = new StringReader(certChainPem);
                var pemReader = new PemReader(reader);
                object? obj;
                while ((obj = pemReader.ReadObject()) != null)
                {
                    if (obj is X509Certificate chainCert)
                    {
                        chainCertificates.Add(chainCert);
                    }
                }
            }

            // Build the chain array: first element is the certificate, followed by the chain certificates.
            var certEntry = new X509CertificateEntry(bcCert);
            var chainEntries = new List<X509CertificateEntry> { certEntry };
            foreach (var chainCert in chainCertificates)
            {
                chainEntries.Add(new X509CertificateEntry(chainCert));
            }

            // Add the private key and associated certificate chain to the store.
            var store = new Pkcs12StoreBuilder().Build();
            string friendlyName = bcCert.SubjectDN.ToString();
            store.SetKeyEntry(friendlyName, new AsymmetricKeyEntry(bcPrivateKey), [.. chainEntries]);

            // Export the store to a password protected PFX byte array.
            using var stream = new MemoryStream();
            store.Save(stream, password.ToCharArray(), new SecureRandom());
            return stream.ToArray();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unable to create a PFX file from the provided certificate, chain and private key");
            throw;
        }
    }
}
