using Azure.Storage.Blobs;
using CryptopackProcessor.Generators;
using CryptopackProcessor.Models;
using CryptopackProcessor.Processors;
using CryptopackProcessor.Uploaders;
using CryptopackProcessor.Validators;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true);
    })
    .ConfigureServices((hostContext, services) =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.AddSingleton(new BlobServiceClient(Environment.GetEnvironmentVariable("StorageConnectionString")));
        services.AddScoped<IManifestFileValidator, KeyIdValidator>();
        services.AddScoped<IManifestFileValidator, KeyValidator>();
        services.AddScoped<IManifestFileValidator, EllipticKeyValidator>();
        services.AddScoped<IManifestFileValidator, CertificateValidator>();
        services.AddScoped<IManifestValidator, ManifestValidator>();
        services.AddScoped<IKeyVaultUploader, KeyVaultUploader>();
        services.AddScoped<EllipticalPfxGenerator>();
        services.AddScoped<PfxGenerator>();
        services.AddScoped<ICryptoProcessor, CryptoProcessor>();
        services.AddScoped<ICertificateGeneratorFactory, CertificateGeneratorFactory>();
        services.Configure<Manifest>(hostContext.Configuration.GetSection("Manifest"));
        services.Configure<KeyVaultSettings>(hostContext.Configuration.GetSection("KeyVaultSettings"));
        services.Configure<AzureAdSettings>(hostContext.Configuration.GetSection("KeyVaultSettings"));
        services.Configure<CryptopackSettings>(hostContext.Configuration.GetSection("CryptopackSettings"));
        services.Configure<WebAppSettings>(hostContext.Configuration.GetSection("WebAppSettings"));
        services.AddScoped<SettingsContainer>();
    })
    .Build();

host.Run();
