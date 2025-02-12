namespace CryptopackProcessor.Models;

public class Manifest
{
    public string KeyId { get; set; } = string.Empty;

    public string MtlsCertificate { get; set; } = string.Empty;

    public string MtlsChain { get; set; } = string.Empty;

    public KeyPair CertificatePair { get; set; } = new KeyPair();

    public KeyPair JwtPair { get; set; } = new KeyPair();
}
