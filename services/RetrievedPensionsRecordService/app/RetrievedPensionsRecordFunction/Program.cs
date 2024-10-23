using MhpdCommon.Extensions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
    })
    .Build();

await host.RunAsync();
