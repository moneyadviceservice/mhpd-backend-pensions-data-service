using MhpdCommon.Extensions;
using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.Models.MHPDModels;
using MhpdCommon.Models.OpenApi;
using MhpdCommon.Repository;
using MhpdCommon.TokenValidation;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.OpenApi;
using PensionsDataService.HttpClients;
using PensionsDataService.Utilities;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<PensionServiceClients>();
builder.Services.AddScoped<PensionServiceUtilities>();
builder.Services.AddScoped<IRetrievalRecordServiceClient, RetrievalRecordServiceClient>();
builder.Services.AddScoped<IRetrievedPensionsRecordClient, RetrievedPensionsRecordClient>();
builder.Services.AddScoped<IPensionAnonymizer, PensionAnonymizer>();
builder.Services.AddScoped<ICardDataRuleEngine, CardDataRuleEngine>();
builder.Services.AddScoped<ISummaryDataRuleEngine, SummaryDataRuleEngine>();
builder.Services.AddScoped<IDetailDataRuleEngine, DetailDataRuleEngine>();
builder.Services.AddScoped<ITimelineSeriesBuilder, TimelineSeriesBuilder>();
builder.Services.AddScoped<ITimelineArrangementFactory, TimelineArrangementFactory>();
builder.Services.AddScoped<IPensionNavigator, PensionNavigator>();
builder.Services.AddScoped<ITokenRequestValidator<PensionsDataRequestModel>, AuthorisationCodeInvalidFormatValidationPensionsData>();
builder.Services.AddScoped<ITokenRequestValidator<PensionsDataRequestModel>, AuthorisationCodeNotPresentValidationPensionsData>();
builder.Services.AddScoped<ITokenRequestValidator<PensionsDataRequestModel>, RedirectUrlNotValidUrlValidationPensionsData>();
builder.Services.AddScoped<ITokenRequestValidator<PensionsDataRequestModel>, RedirectUrlNotPresentValidationPensionsData>();
builder.Services.AddScoped<ITokenRequestValidator<PensionsDataRequestModel>, CodeVerifierNotPresentValidationPensionsData>();
builder.Services.AddScoped<ITokenRequestValidator<PensionsDataRequestModel>, CodeVerifierNotBase64StringPensionsData>();
builder.Services.AddScoped<ITokenRequestValidator<PensionsDataRequestModel>, ClientIdInvalidFormatValidationPensionData>();
builder.Services.AddScoped<ITokenRequestValidator<PensionsDataRequestModel>, ClientIdNotPresentValidationPensionData>();
builder.Services.AddScoped<ITokenRequestValidator<PensionsDataRequestModel>, ClientSecretNotGuidValidationPensionData>();
builder.Services.AddScoped<ITokenRequestValidator<PensionsDataRequestModel>, ClientSecretNotPresentValidationPensionData>();
builder.Services.AddScoped<PensionsDataRequestValidatorPipeline>();
builder.Services.AddMhpdHttpClients();
builder.Services.AddCommonConfigurations();
builder.Services.AddIntegrationServices();
builder.Services.AddMhpdUtilities();
builder.Services.AddMhpdCosmosDb(builder.Configuration);
builder.Services.AddAntiForgeryValidation();

builder.Services.AddApplicationInsightsTelemetry();
builder.Logging.AddMhpdTelemetry(builder.Configuration);

builder.Services.AddScoped<ICosmosDbRepository<UserSessionData>, UserSessionDataRepository>();

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.DefaultIgnoreCondition =
        JsonIgnoreCondition.WhenWritingNull;
});

builder.Services.AddHttpLogging(logging =>
{
    logging.LoggingFields = HttpLoggingFields.All;    
    logging.RequestBodyLogLimit = 4096;
    logging.ResponseBodyLogLimit = 4096;
    logging.CombineLogs = true;
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.DocumentFilter<CsrfEndpointFilter>();
    c.AddServer(new OpenApiServer
    {
        Url = builder.Configuration.GetValue<string>("OpenApiServerUrl") ?? "https://localhost:3000"
    });
});
builder.Services.AddHttpClient();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowDashboard", policy =>
    {
        policy.WithOrigins("http://localhost:5500", "http://127.0.0.1:5500", "https://mhpddev.z33.web.core.windows.net/")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(c => c.OpenApiVersion = OpenApiSpecVersion.OpenApi2_0);
    app.UseSwaggerUI();
}

app.UseCors("AllowDashboard");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.UseHttpLogging();
app.UseMhpdSecureEndpoints();

await app.RunAsync();

[ExcludeFromCodeCoverage]
public static partial class Program { }