namespace CryptopackProcessor.Uploaders;

public interface IKeyVaultUploader
{
    Task UploadCertificateAsync(byte[] pfxBytes);

    Task UploadSecretsAsync(string jwtPrivateKey, string kid);
}
