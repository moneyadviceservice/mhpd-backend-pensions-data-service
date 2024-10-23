using MhpdCommon.Extensions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PensionsRetrievalFunction.HttpClients;
using PensionsRetrievalFunction.Models;
using PensionsRetrievalFunction.Orchestration;
using PensionsRetrievalFunction.Repository;

var tryParseConfig = new Func<string?, int, int>((value, defaultValue) =>
{
    if(int.TryParse(value, out var result)) return result;
    return defaultValue;
});

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
        services.AddScoped<IPensionRetrievalRepository, PensionRetrievalRepository>();
        services.AddTransient<IPeiServiceClient, PeiServiceClient>();
        services.AddTransient<IPeiIntegrationOrchestrator, PeiIntegrationOrchestrator>();
        services.AddOptions<PeiOrchestrationSettings>().Configure(option =>
        {
            option.PeiRetryTimeout = tryParseConfig(Environment.GetEnvironmentVariable(PeiOrchestrationSettings.PeiRetryTimeoutVariable), 
                PeiOrchestrationSettings.MaxRetryDuration);
            option.PeiRetryInterval = tryParseConfig(Environment.GetEnvironmentVariable(PeiOrchestrationSettings.PeiRetryIntervalVariable), 
                PeiOrchestrationSettings.MinRetryInterval);
        }).ValidateOnStart();
    })
    .Build();

host.Run();
