namespace CryptopackProcessor.Models;

public static class Constants
{
    public static class SecurityKey
    {
        public const string CertificateStart = "-----BEGIN CERTIFICATE-----";
        public const string CertificateEnd = "-----END CERTIFICATE-----";
        public const string PublicKeyStart = "-----BEGIN PUBLIC KEY-----";
        public const string PublicKeyEnd = "-----END PUBLIC KEY-----";
        public const string PrivateKeyStart = "-----BEGIN PRIVATE KEY-----";
        public const string PrivateKeyEnd = "-----END PRIVATE KEY-----";
    }

    public static class FileHandling
    {
        public const string ArchivePath = "archive";
        public const string TargetContainer = "drop";
        public const string ContentType = "application/octet-stream";
    }
}
