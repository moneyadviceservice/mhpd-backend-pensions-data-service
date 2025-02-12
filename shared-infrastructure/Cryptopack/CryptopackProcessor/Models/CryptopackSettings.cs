namespace CryptopackProcessor.Models;

public class CryptopackSettings
{
    public string PfxPassword { get; set; } = string.Empty;

    public string MtlsCertificateName { get; set; } = string.Empty;

    public string PrivateKeySecretName { get; set; } = string.Empty;

    public string KidSecretName { get; set; } = string.Empty;
}
