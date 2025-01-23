using MhpdCommon.Extensions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Configurations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using RetrievedPensionsRecordFunction.Repository;
using RetrievedPensionsRecordFunction.Utils;

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
        services.AddMhpdCosmosDb(hostContext.Configuration);
        services.AddMhpdUtilities();
        services.AddScoped<IPensionRecordValidator, PensionRecordValidator>();
        services.AddScoped<IPensionRecordRepository, PensionRecordRepository>();
        services.AddSingleton<IOpenApiConfigurationOptions>(_ =>
        {
            var options = new OpenApiConfigurationOptions()
            {
                Info = new OpenApiInfo
                {
                    Title = "MaPS Retrieved Pension Records",
                    Version = DefaultOpenApiConfigurationOptions.GetOpenApiDocVersion(),
                    Description =
                        "This service allows a client to retrieve retrieved pension records related to a user session",
                    Contact = new OpenApiContact
                    {
                        Name = "General Enquires",
                        Email = "contact@maps.org.uk",
                        Url = new Uri("https://maps.org.uk/en/about-us/contact-us")
                    },
                    License = new OpenApiLicense
                    {
                        Name = "Government API License",
                        Url = new Uri("https://www.nationalarchives.gov.uk/doc/open-government-licence/version/3/")
                    },
                },
                OpenApiVersion = Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums.OpenApiVersionType.V3
            };

            return options;
        });
    })
    .Build();

await host.RunAsync();
