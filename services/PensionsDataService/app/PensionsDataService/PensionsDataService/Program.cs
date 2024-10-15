using System.Diagnostics.CodeAnalysis;
using MhpdCommon.Extensions;
using MhpdCommon.Models.Configuration;
using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.TokenValidation;
using MhpdCommon.Utils;
using Microsoft.AspNetCore.HttpLogging;
using PensionsDataService.HttpClients;

var builder = WebApplication.CreateBuilder(args);

// Load environment-specific configurations
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())    // Optional, ensures you're loading from the correct directory
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// Check if running in development environment
if (builder.Environment.IsDevelopment())
{
    // Access the connection string from configuration
    var serviceBusConnection = builder.Configuration.GetConnectionString("ServiceBusConnectionString");
    var outboundQueue = builder.Configuration.GetSection("CommonServiceBusConfiguration")["OutboundQueue"];

    // Set the value as an environment variable (for this process)
    Environment.SetEnvironmentVariable("ServiceBusConnectionString", serviceBusConnection);
    Environment.SetEnvironmentVariable("OutboundQueue", outboundQueue);
    
    Console.WriteLine("ServiceBusConnectionString {0}", serviceBusConnection);
    Console.WriteLine("OutboundQueue {0}", outboundQueue);
}

Console.WriteLine("ServiceBusConnectionString {0}", builder.Configuration.GetConnectionString("ServiceBusConnectionString"));
Console.WriteLine("OutboundQueue {0}", builder.Configuration.GetSection("OutboundQueue"));

// Add services to the container.
builder.Services.AddScoped<IIdValidator, IdValidator>();
builder.Services.AddScoped<ITokenIntegrationServiceClient, TokenIntegrationServiceClient>();
builder.Services.AddScoped<IRetrievalRecordFunctionClient, RetrievalRecordFunctionClient>();

builder.Services.AddScoped<ITokenRequestValidator<PensionsDataRequestModel>, AuthorisationCodeInvalidFormatValidationPensionsData>();
builder.Services.AddScoped<ITokenRequestValidator<PensionsDataRequestModel>, AuthorisationCodeNotPresentValidationPensionsData>();
builder.Services.AddScoped<ITokenRequestValidator<PensionsDataRequestModel>, RedirectUriNotValidUrlValidationPensionsData>();
builder.Services.AddScoped<ITokenRequestValidator<PensionsDataRequestModel>, RedirectUriNotPresentValidationPensionsData>();
builder.Services.AddScoped<ITokenRequestValidator<PensionsDataRequestModel>, CodeVerifierNotPresentValidationPensionsData>();
builder.Services.AddScoped<ITokenRequestValidator<PensionsDataRequestModel>, CodeVerifierNotBase64StringPensionsData>();
builder.Services.AddScoped<PensionsDataRequestValidatorPipeline>();
builder.Services.AddMhpdServiceBusTools();

builder.Services.AddControllers();

builder.Services.AddHttpLogging(logging =>
{
    logging.LoggingFields = HttpLoggingFields.All;    
    logging.RequestBodyLogLimit = 4096;
    logging.ResponseBodyLogLimit = 4096;
    logging.CombineLogs = true;
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient(); 

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseHttpLogging();

app.Run();

[ExcludeFromCodeCoverage]
public partial class Program { }