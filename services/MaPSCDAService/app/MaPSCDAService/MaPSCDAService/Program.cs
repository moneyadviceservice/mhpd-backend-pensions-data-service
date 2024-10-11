using System.Diagnostics.CodeAnalysis;
using Azure.Identity;
using MaPSCDAService;
using MaPSCDAService.Utils;
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

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseHttpLogging();
app.Run();

[ExcludeFromCodeCoverage]
public partial class Program { }