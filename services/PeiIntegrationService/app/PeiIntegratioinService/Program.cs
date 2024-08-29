using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.HttpLogging;
using PeiIntegrationService.HttpClients.Implementation;
using PeiIntegrationService.HttpClients.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// add appsettings.json
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

builder.Services.AddHttpClient<CdaPiesServiceClient>("CdaPiesService", c => { });
builder.Services.AddHttpClient<MapsCdaServiceClient>("MapsCdaService", c => { });
builder.Services.AddHttpClient<MapsCdaServiceClient>("TokenIntegrationService", c => { });

// Add services to the container.
builder.Services.AddTransient<ICdaPiesServiceClient, CdaPiesServiceClient>();
builder.Services.AddTransient<IMapsRqpServiceClient, MapsCdaServiceClient>();
builder.Services.AddTransient<ITokenIntegrationServiceClient, TokenIntegrationServiceClient>();

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpLogging(logging =>
{
    logging.LoggingFields = HttpLoggingFields.All;
    logging.RequestHeaders.Add("cdaUserGuid");
    logging.RequestHeaders.Add("iss");
    logging.RequestHeaders.Add("userSessionId");
    logging.ResponseHeaders.Add("rpt");
    logging.MediaTypeOptions.AddText("application/json");
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

