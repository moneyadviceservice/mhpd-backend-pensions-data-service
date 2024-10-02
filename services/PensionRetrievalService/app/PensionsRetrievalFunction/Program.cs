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
        services.AddMhpdCosmosDb();
        services.AddMhpdUtilities();
        services.AddMhpdServiceBusTools();
        services.AddScoped<IPensionRetrievalRepository, PensionRetrievalRepository>();
        services.AddTransient<IPeiServiceClient, PeiServiceClient>();
        services.AddTransient<IPeiIntegrationOrchestrator, PeiIntegrationOrchestrator>();
        services.AddOptions<MhpdApiConfiguration>().Configure(option =>
        {
            option.PeiIntegrationApi = Environment.GetEnvironmentVariable(MhpdApiConfiguration.PeiIntegrationApiVariable);
            option.PeiRetryTimeout = tryParseConfig(Environment.GetEnvironmentVariable(MhpdApiConfiguration.PeiRetryTimeoutVariable), 
                MhpdApiConfiguration.MaxRetryDuration);
            option.PeiRetryInterval = tryParseConfig(Environment.GetEnvironmentVariable(MhpdApiConfiguration.PeiRetryIntervalVariable), 
                MhpdApiConfiguration.MinRetryInterval);
        }).ValidateOnStart();
    })
    .Build();

host.Run();
