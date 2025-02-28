using CryptopackProcessor.Models;
using Microsoft.Extensions.DependencyInjection;

namespace CryptopackProcessor.Generators;

public class CertificateGeneratorFactory(IServiceProvider serviceProvider) : ICertificateGeneratorFactory
{
    public IPfxGenerator GetGenerator(KeyAlgorithmType algorithmType)
    {
        return algorithmType switch
        {
            KeyAlgorithmType.EC => serviceProvider.GetRequiredService<EllipticalPfxGenerator>(),
            KeyAlgorithmType.RSA => serviceProvider.GetRequiredService<PfxGenerator>(),
            _ => throw new ArgumentException("Invalid service type", nameof(algorithmType))
        };
    }
}
