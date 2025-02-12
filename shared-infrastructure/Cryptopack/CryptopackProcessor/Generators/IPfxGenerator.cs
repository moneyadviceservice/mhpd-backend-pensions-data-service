namespace CryptopackProcessor.Generators;

public interface IPfxGenerator
{
    byte[]? GeneratePfx(string certPem, string privateKeyPem, string? certChainPem, string password);
}
