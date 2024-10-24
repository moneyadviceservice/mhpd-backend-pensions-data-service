using System.Diagnostics.CodeAnalysis;
using MhpdCommon.Extensions;
using MhpdCommon.Models.Configuration;
using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.Models.MHPDModels;
using MhpdCommon.TokenValidation;
using MhpdCommon.Utils;
using Microsoft.AspNetCore.HttpLogging;
using PensionsDataService.HttpClients;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<IIdValidator, IdValidator>();
builder.Services.AddScoped<PensionServiceClients>();
builder.Services.AddScoped<ITokenIntegrationServiceClient, TokenIntegrationServiceClient>();
builder.Services.AddScoped<IRetrievalRecordServiceClient, RetrievalRecordServiceClient>();
builder.Services.AddScoped<IRetrievedPensionsRecordClient, RetrievedPensionsRecordClient>();

builder.Services.AddScoped<ITokenRequestValidator<PensionsDataRequestModel>, AuthorisationCodeInvalidFormatValidationPensionsData>();
builder.Services.AddScoped<ITokenRequestValidator<PensionsDataRequestModel>, AuthorisationCodeNotPresentValidationPensionsData>();
builder.Services.AddScoped<ITokenRequestValidator<PensionsDataRequestModel>, RedirectUriNotValidUrlValidationPensionsData>();
builder.Services.AddScoped<ITokenRequestValidator<PensionsDataRequestModel>, RedirectUriNotPresentValidationPensionsData>();
builder.Services.AddScoped<ITokenRequestValidator<PensionsDataRequestModel>, CodeVerifierNotPresentValidationPensionsData>();
builder.Services.AddScoped<ITokenRequestValidator<PensionsDataRequestModel>, CodeVerifierNotBase64StringPensionsData>();
builder.Services.AddScoped<PensionsDataRequestValidatorPipeline>();
builder.Services.AddMhpdServiceBusTools();
builder.Services.AddMhpdHttpClients();

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