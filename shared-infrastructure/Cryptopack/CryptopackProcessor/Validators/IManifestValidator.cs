using CryptopackProcessor.Models;
using System.IO.Compression;

namespace CryptopackProcessor.Validators;

public interface IManifestValidator
{
    ValidationResult Validate(ZipArchive archive);
}
