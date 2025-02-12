namespace CryptopackProcessor.Models;

public class KeyPair
{
    public string PublicKey { get; set; } = string.Empty;

    public string PrivateKey { get; set; } = string.Empty;

    public KeyAlgorithmType AlgorithmType { get; set; }
}
