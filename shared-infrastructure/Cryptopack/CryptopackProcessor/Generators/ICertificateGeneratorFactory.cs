using CryptopackProcessor.Models;

namespace CryptopackProcessor.Generators;

public interface ICertificateGeneratorFactory
{
    IPfxGenerator GetGenerator(KeyAlgorithmType algorithmType);
}
