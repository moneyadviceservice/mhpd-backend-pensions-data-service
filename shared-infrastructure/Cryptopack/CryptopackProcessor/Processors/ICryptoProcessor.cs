using System.IO.Compression;

namespace CryptopackProcessor.Processors;

public interface ICryptoProcessor
{
    Task ProcessAsync(ZipArchive archive);
}
