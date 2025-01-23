using System.Diagnostics.CodeAnalysis;
using Azure.Identity;
using MaPSCDAService;
using MaPSCDAService.Utils;
using MhpdCommon.Extensions;
using Microsoft.AspNetCore.HttpLogging;

var builder = WebApplication.CreateBuilder(args);

if (!builder.Environment.IsDevelopment())
{
    builder.Configuration.AddAzureKeyVault(
        new Uri(builder.Configuration.GetSection("KeyVaultConfiguration")["KeyVaultURL"]!),
        new DefaultAzureCredential()
    );
}

builder.Services.AddOptions<UriSettings>()
    .Bind(builder.Configuration.GetSection("UriSettings"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// Add services to the container.
builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddMhpdUtilities();
builder.Services.AddScoped<IRqpTokenManager, RqpTokenManager>();
builder.Services.AddControllers();
builder.Services.AddTransient<IPkceGenerator, PkceGenerator>();
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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseHttpLogging();
app.Run();

[ExcludeFromCodeCoverage]
public partial class Program { }