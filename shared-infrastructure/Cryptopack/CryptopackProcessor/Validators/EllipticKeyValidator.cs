using CryptopackProcessor.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.OpenSsl;
using System.IO.Compression;
using System.Text;

namespace CryptopackProcessor.Validators;

public class EllipticKeyValidator(ILogger<EllipticKeyValidator> logger, IOptions<Manifest> options) : IManifestFileValidator
{
    private readonly Manifest _manifest = options.Value;

    public ValidationResult Validate(ZipArchive archive)
    {
        var keyPair = _manifest.CertificatePair;
        var privateKeyfile = archive.Entries.SingleOrDefault(file => file.Name == keyPair.PrivateKey);
        var publicKeyfile = archive.Entries.SingleOrDefault(file => file.Name == keyPair.PublicKey);

        if (privateKeyfile == null || publicKeyfile == null)
        {
            logger.LogInformation("Matching pair not found for {private} and {public}", keyPair.PrivateKey, keyPair.PublicKey);
            return new ValidationResult();
        }

        using var privateKeyStream = privateKeyfile.Open();
        using var publicKeyStream = publicKeyfile.Open();
        using var privateKeyReader = new StreamReader(privateKeyStream);
        using var publicKeyReader = new StreamReader(publicKeyStream);

        var privateKeyContent = privateKeyReader.ReadToEnd().ReplaceLineEndings(string.Empty);
        var publicKeyContent = publicKeyReader.ReadToEnd().ReplaceLineEndings(string.Empty);

        var isKeyPairValid = VerifyECKey(privateKeyContent, publicKeyContent);

        var verificationMessage = $"Key pair: '{keyPair.PrivateKey}' and '{keyPair.PublicKey}' {(isKeyPairValid ? "have been" : "could not be")} verified";
        logger.LogInformation("{message}", verificationMessage);

        return new ValidationResult { IsValid = isKeyPairValid };
    }

    private static bool VerifyECKey(string privateKeyContent, string publicKeyContent)
    {
        AsymmetricKeyParameter privateKey = ReadAsymmetricKey(privateKeyContent, true);
        AsymmetricKeyParameter publicKey = ReadAsymmetricKey(publicKeyContent, false);

        byte[] testData = Encoding.UTF8.GetBytes("Test data");
        byte[] hash = ComputeSha256Hash(testData);

        // Sign the hash with the private key.
        var signer = new ECDsaSigner();
        signer.Init(true, privateKey);
        BigInteger[] signatureComponents = signer.GenerateSignature(hash);

        // Verify the signature using the public key.
        var verifier = new ECDsaSigner();
        verifier.Init(false, publicKey);
        bool isValid = verifier.VerifySignature(hash, signatureComponents[0], signatureComponents[1]);

        return isValid;
    }

    private static AsymmetricKeyParameter ReadAsymmetricKey(string pemContent, bool isPrivateKey)
    {
        using var reader = new StringReader(pemContent);
        var pemReader = new PemReader(reader);
        object keyObject = pemReader.ReadObject();

        if (keyObject is AsymmetricCipherKeyPair keyPair)
        {
            return isPrivateKey ? keyPair.Private : keyPair.Public;
        }
        else if (keyObject is AsymmetricKeyParameter keyParam)
        {
            return keyParam;
        }
        else
        {
            throw new InvalidOperationException($"Invalid {(isPrivateKey ? "private" : "public")} key format.");
        }
    }

    private static byte[] ComputeSha256Hash(byte[] data)
    {
        var digest = new Sha256Digest();
        digest.BlockUpdate(data, 0, data.Length);
        byte[] hash = new byte[digest.GetDigestSize()];
        digest.DoFinal(hash, 0);
        return hash;
    }
}
