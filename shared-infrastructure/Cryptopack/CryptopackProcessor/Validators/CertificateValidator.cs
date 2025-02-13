using CryptopackProcessor.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.IO.Compression;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace CryptopackProcessor.Validators;

public class CertificateValidator(ILogger<CertificateValidator> logger, IOptions<Manifest> options) : IManifestFileValidator
{
    private readonly Manifest _manifest = options.Value;

    public ValidationResult Validate(ZipArchive archive)
    {
        var isCertificateValid = ValidateCertificate(_manifest.MtlsCertificate, archive);

        isCertificateValid &= ValidateCertificate(_manifest.MtlsChain, archive);

        return new ValidationResult { IsValid = isCertificateValid };
    }

    private bool ValidateCertificate(string certName, ZipArchive archive)
    {
        var certfile = archive.Entries.SingleOrDefault(file => file.Name == certName);

        if (certfile == null)
        {
            logger.LogWarning("Expected certificate {cert} not found in the pack", certName);
            return false;
        }

        using var certStream = certfile.Open();
        using var certReader = new StreamReader(certStream);

        var certContent = certReader.ReadToEnd();

        return ValidateCertificate(certName, certContent);
    }

    private bool ValidateCertificate(string certName, string certificateData)
    {
        try
        {
            var cert = new X509Certificate2(Encoding.UTF8.GetBytes(certificateData));

            if (DateTime.Now < cert.NotBefore || DateTime.Now > cert.NotAfter)
            {
                logger.LogWarning("Certificate '{cert}' is either expired or not yet active. Valid from {from} - Valid to {to}", 
                    certName, cert.NotBefore, cert.NotAfter);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Certificate validation error");
            return false;
        }
    }
}
