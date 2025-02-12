using System.IO.Compression;

namespace CryptopackProcessor.Extensions;

public static class ZipArchiveExtensions
{
    public static string GetFileContents(this ZipArchive archive, string fileName)
    {
        var zipEntry = archive.Entries.SingleOrDefault(file => file.Name == fileName);

        if (zipEntry == null)
        {
            throw new FileNotFoundException("Expected certificate {cert} not found in the pack", fileName);
        }

        using var stream = zipEntry.Open();
        using var reader = new StreamReader(stream);

        return reader.ReadToEnd();
    }
}
