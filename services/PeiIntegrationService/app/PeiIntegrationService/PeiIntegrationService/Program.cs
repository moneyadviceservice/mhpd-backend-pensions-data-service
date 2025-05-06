using System.Diagnostics.CodeAnalysis;
using MhpdCommon.Constants;
using MhpdCommon.Extensions;
using Microsoft.AspNetCore.HttpLogging;
using PeiIntegrationService.HttpClients.Implementation;
using PeiIntegrationService.HttpClients.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// add appsettings.json
builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);

// Add services to the container.
builder.Services.AddTransient<ICdaPiesServiceClient, CdaPeisServiceClient>();

builder.Services.AddMhpdUtilities();
builder.Services.AddMhpdHttpClients(builder.Configuration);
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddApplicationInsightsTelemetry();

builder.Services.AddHttpLogging(logging =>
{
    logging.LoggingFields = HttpLoggingFields.All;
    logging.RequestHeaders.Add(HeaderConstants.PeisId);
    logging.RequestHeaders.Add(HeaderConstants.Iss);
    logging.RequestHeaders.Add(HeaderConstants.UserSessionId);
    logging.ResponseHeaders.Add(HeaderConstants.Rpt);
    logging.MediaTypeOptions.AddText("application/json");
    logging.RequestBodyLogLimit = 4096;
    logging.ResponseBodyLogLimit = 4096;
    logging.CombineLogs = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(c => c.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi2_0);
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();
app.UseHttpLogging();
app.Run();

[ExcludeFromCodeCoverage]
public partial class Program { }

