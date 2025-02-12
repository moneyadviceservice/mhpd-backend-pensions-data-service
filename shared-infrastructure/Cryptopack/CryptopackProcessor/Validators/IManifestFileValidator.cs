using CryptopackProcessor.Models;
using System.IO.Compression;
using System.Text;

namespace CryptopackProcessor.Validators;
public interface IManifestFileValidator
{
    ValidationResult Validate(ZipArchive archive);
}
