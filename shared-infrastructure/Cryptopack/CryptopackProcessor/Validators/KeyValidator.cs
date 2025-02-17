using CryptopackProcessor.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

namespace CryptopackProcessor.Validators;

public class KeyValidator(ILogger<KeyValidator> logger, IOptions<Manifest> options) : IManifestFileValidator
{
    private readonly Manifest _manifest = options.Value;

    public ValidationResult Validate(ZipArchive archive)
    {
        var isKeyPairValid = ValidateKeyPair(_manifest.JwtPair, archive);

        return new ValidationResult { IsValid = isKeyPairValid };
    }

    private bool ValidateKeyPair(KeyPair keyPair, ZipArchive archive)
    {
        var privateKeyfile = archive.Entries.SingleOrDefault(file => file.Name == keyPair.PrivateKey);
        var publicKeyfile = archive.Entries.SingleOrDefault(file => file.Name == keyPair.PublicKey);

        if (privateKeyfile == null || publicKeyfile == null)
        {
            logger.LogInformation("Matching pair not found for {private} and {public}", keyPair.PrivateKey, keyPair.PublicKey);
            return false;
        }

        using var privateKeyStream = privateKeyfile.Open();
        using var publicKeyStream = publicKeyfile.Open();
        using var privateKeyReader = new StreamReader(privateKeyStream);
        using var publicKeyReader = new StreamReader(publicKeyStream);

        var privateKeyContent = privateKeyReader.ReadToEnd().ReplaceLineEndings(string.Empty);
        var publicKeyContent = publicKeyReader.ReadToEnd().ReplaceLineEndings(string.Empty);

        var isValid = VerifyKeyPair(keyPair, privateKeyContent, publicKeyContent);

        var verificationMessage = $"Key pair: '{keyPair.PrivateKey}' and '{keyPair.PublicKey}' {(isValid ? "have been" : "could not be")} verified";
        logger.LogInformation("{message}", verificationMessage);

        return isValid;
    }

    private bool VerifyKeyPair(KeyPair keyPair, string privateKeyContent, string publicKeyContent)
    {
        try
        {
            byte[] testData = Encoding.UTF8.GetBytes("Test data");

            return keyPair.AlgorithmType switch
            {
                KeyAlgorithmType.RSA => VerifyRSAKey(privateKeyContent, publicKeyContent, testData),
                KeyAlgorithmType.EC => VerifyECKey(privateKeyContent, publicKeyContent, testData),
                _ => throw new InvalidOperationException("Cryptgraphy key algortithm is not recognised"),
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error verifying {private} and {public}", keyPair.PrivateKey, keyPair.PublicKey);
            return false;
        }
    }

    private static bool VerifyRSAKey(string privateKeyContent, string publicKeyContent, byte[] testData)
    {
        byte[] signature;

        using (var rsaPrivate = RSA.Create())
        {
            rsaPrivate.ImportFromPem(privateKeyContent.ToCharArray());
            signature = rsaPrivate.SignData(testData, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }

        using var rsa = RSA.Create();
        rsa.ImportFromPem(publicKeyContent.ToCharArray());

        // Verify data with public key
        return rsa.VerifyData(testData, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
    }

    private static bool VerifyECKey(string privateKeyContent, string publicKeyContent, byte[] testData)
    {
        byte[] signature;

        using (var rsaPrivate = ECDsa.Create(ECCurve.NamedCurves.nistP256))
        {
            rsaPrivate.ImportFromPem(privateKeyContent.ToCharArray());
            signature = rsaPrivate.SignData(testData, HashAlgorithmName.SHA256);
        }

        using var rsa = ECDsa.Create();
        rsa.ImportFromPem(publicKeyContent.ToCharArray());

        // Verify data with public key
        return rsa.VerifyData(testData, signature, HashAlgorithmName.SHA256);
    }
}
