using System.Diagnostics.CodeAnalysis;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PensionRequestFunction.HttpClient;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.AddHttpClient();
        services.AddAzureClients(builder =>
        {
            builder.AddServiceBusClient(Environment.GetEnvironmentVariable("connectionstring"));
        });
        services.AddSingleton<IPDPViewDataClient, PDPViewDataClient>();
    })
    .Build();

host.Run();

[ExcludeFromCodeCoverage]
public partial class Program { }