using CryptopackProcessor.Models;

namespace CryptopackProcessor.Extensions;

public static class SecurityKeyExtensions
{
    public static string Sanitized(this string pemString)
    {
        return pemString
            .Replace(Constants.SecurityKey.CertificateStart, string.Empty)
            .Replace(Constants.SecurityKey.CertificateEnd, string.Empty)
            .Replace(Constants.SecurityKey.PublicKeyStart, string.Empty)
            .Replace(Constants.SecurityKey.PublicKeyEnd, string.Empty)
            .Replace(Constants.SecurityKey.PrivateKeyStart, string.Empty)
            .Replace(Constants.SecurityKey.PrivateKeyEnd, string.Empty)
            .Flat();
    }

    public static string Flat(this string pemString)
    {
        return pemString
            .Replace("\n", string.Empty).Replace("\r", string.Empty);
    }
}
