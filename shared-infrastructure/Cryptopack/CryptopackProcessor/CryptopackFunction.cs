using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using CryptopackProcessor.Models;
using CryptopackProcessor.Processors;
using CryptopackProcessor.Validators;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.IO.Compression;

namespace CryptopackProcessor;

public class CryptopackFunction(ILogger<CryptopackFunction> logger, BlobServiceClient blobServiceClient, 
    IOptions<Manifest> options, IManifestValidator validator, ICryptoProcessor processor)
{
    private readonly Manifest _manifest = options.Value;
    private readonly BlobContainerClient _containerClient = blobServiceClient.GetBlobContainerClient(Constants.FileHandling.TargetContainer);

    [Function(nameof(CryptopackFunction))]
    public async Task Run([BlobTrigger(Constants.FileHandling.TargetContainer + "/{name}.zip", Connection = "StorageConnectionString")] Stream stream, string name)
    {
        try
        {
            logger.LogWarning("Detected potential cryptopack: '{name}'. Processing...", name);

            // Validate the contents of the pack
            using var archive = new ZipArchive(stream);
            var validationResult = validator.Validate(archive);

            if (validationResult.IsValid)
            {
                // Generate and upload the required content to the vault
                await processor.ProcessAsync(archive);
            }

            // Unwrap and archive the contents of the zip
            await ArchiveManifest(archive, name, validationResult.IsValid);

            // Delete the original zip file so the next upload can be processed
            var blobClient = _containerClient.GetBlobClient($"/{name}.zip");
            await blobClient.DeleteAsync();
            logger.LogWarning("Archived and removed original zip file: {name}", name);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing crypto pack");
        }
    }

    private async Task ArchiveManifest(ZipArchive archive, string name, bool isValid)
    {
        logger.LogInformation("Archiving zip contents...");
        var timestamp = DateTime.UtcNow;

        foreach (var entry in archive.Entries)
        {
            if(string.IsNullOrEmpty(entry.Name)) continue;

            logger.LogInformation("Unwrapped zip entry: {entry}", entry.FullName);
            using var entryStream = entry.Open();

            var blobClient = _containerClient.GetBlobClient(
                $"{Constants.FileHandling.ArchivePath}/{name}_{timestamp: yyyy_MM_dd_HHmmss}_{(isValid ? "valid": "invalid")}/{entry.Name}");
            await blobClient.UploadAsync(entryStream, new BlobHttpHeaders { ContentType = Constants.FileHandling.ContentType });
            logger.LogInformation("Uploaded {entry} to {path}", entry.FullName, Constants.FileHandling.ArchivePath);
        }
    }
}
