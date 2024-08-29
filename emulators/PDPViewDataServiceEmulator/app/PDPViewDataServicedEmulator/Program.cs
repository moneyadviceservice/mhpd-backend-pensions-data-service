using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Azure.Cosmos;
using PDPViewDataServicedEmulator.CosmosRepository;
using PDPViewDataServicedEmulator.Mocks;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddSingleton<ICosmosDbRepository<ViewDataPayload>>(serviceProvider =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();

    var account = configuration.GetSection("AzureCosmosDb")["Account"];
    var key = configuration.GetSection("AzureCosmosDb")["Key"];
    var databaseName = configuration.GetSection("AzureCosmosDb")["DatabaseName"];
    var containerName = configuration["AzureCosmosDb:ContainerName"];

    var cosmosClient = new CosmosClient(account, key);
    
    // Once Managed identity works uncomment this block and comment line 21
    //CosmosClient cosmosClient = new(
    //    accountEndpoint: configuration.GetSection("AzureCosmosDb")["Account"],
    //    tokenCredential: new DefaultAzureCredential()
    //    );

    return new CosmosDbRepository<ViewDataPayload>(cosmosClient, databaseName!, containerName!);
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpLogging(logging =>
{
    logging.LoggingFields = HttpLoggingFields.All;
    logging.RequestHeaders.Add("X-Request-ID");    
    logging.ResponseHeaders.Add("WWW-Authenticate");
    logging.RequestBodyLogLimit = 4096;
    logging.ResponseBodyLogLimit = 4096;
    logging.CombineLogs = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();
app.UseHttpLogging();
app.Run();

[ExcludeFromCodeCoverage]
public partial class Program { }
