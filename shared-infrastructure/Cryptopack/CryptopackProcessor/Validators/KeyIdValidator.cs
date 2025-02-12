using CryptopackProcessor.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.IO.Compression;

namespace CryptopackProcessor.Validators;

public class KeyIdValidator(ILogger<KeyIdValidator> logger, IOptions<Manifest> options) : IManifestFileValidator
{
    private readonly Manifest _manifest = options.Value;

    public ValidationResult Validate(ZipArchive archive)
    {
        var kidfile = archive.Entries.SingleOrDefault(file => file.Name == _manifest.KeyId);

        if (kidfile == null)
        {
            logger.LogWarning("Expected Key Id file {kid} not found in the pack", _manifest.KeyId);
            return new ValidationResult();
        }

        using var kidStream = kidfile.Open();
        using var kidReader = new StreamReader(kidStream);

        var kid = kidReader.ReadToEnd().ReplaceLineEndings(string.Empty);

        var isKidValid = !string.IsNullOrWhiteSpace(kid) && Guid.TryParse(kid, out _);

        if (!isKidValid)
        {
            logger.LogWarning("Kid content is invalid: {kid}", kid);
        }

        return new ValidationResult { IsValid = isKidValid };
    }
}
