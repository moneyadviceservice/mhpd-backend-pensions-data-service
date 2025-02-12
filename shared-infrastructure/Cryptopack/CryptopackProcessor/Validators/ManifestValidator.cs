using CryptopackProcessor.Models;
using Microsoft.Extensions.Logging;
using System.IO.Compression;

namespace CryptopackProcessor.Validators;

public class ManifestValidator(ILogger<ManifestValidator> logger, IEnumerable<IManifestFileValidator> validators) : IManifestValidator
{
    public ValidationResult Validate(ZipArchive archive)
    {
        if (!validators.Any())
        {
            logger.LogCritical("No validators found.");
            return new ValidationResult();
        }

        bool isManifestValid = true;

        foreach (var validator in validators)
        {
            isManifestValid &= validator.Validate(archive).IsValid;
        }

        return new ValidationResult { IsValid = isManifestValid };
    }
}
