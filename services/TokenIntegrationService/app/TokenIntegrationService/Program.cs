using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.HttpLogging;
using TokenIntegrationService.HttpClients;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient<CDATokenService>("CDAService", c => {} );

builder.Services.AddScoped<ICDATokenService, CDATokenService>();

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddHttpLogging(logging =>
{
    logging.LoggingFields = HttpLoggingFields.All;    
    logging.RequestBodyLogLimit = 4096;
    logging.ResponseBodyLogLimit = 4096;
    logging.CombineLogs = true;
});
var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseAuthorization();
app.MapControllers();
app.UseHttpLogging();
app.Run();

[ExcludeFromCodeCoverage]
public partial class Program { }