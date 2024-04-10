using Microsoft.AspNetCore.HttpLogging;
using PeiIntegratioinService.HttpClients;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient<CDAService>("CDAService", c =>
{

});

// Add services to the container.
builder.Services.AddScoped<ICDAService, CDAService>();

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
