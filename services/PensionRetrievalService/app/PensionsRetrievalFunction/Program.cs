using MhpdCommon.Extensions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PensionsRetrievalFunction.HttpClients;
using PensionsRetrievalFunction.Orchestration;
using PensionsRetrievalFunction.Repository;

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
        services.AddMhpdHttpClients(hostContext.Configuration);
        services.AddMhpdServiceBusTools(hostContext.Configuration);
        services.AddCommonConfigurations(hostContext.Configuration);
        services.AddScoped<IPensionRetrievalRepository, PensionRetrievalRepository>();
        services.AddTransient<IPeiServiceClient, PeiServiceClient>();
        services.AddTransient<IPeiIntegrationOrchestrator, PeiIntegrationOrchestrator>();
    })
    .Build();

host.Run();
