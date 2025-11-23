using EducationCenter.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

var builder = WebApplication.CreateBuilder(args);

// LOGGING
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// DB CONTEXT
builder.Services.AddDbContext<EducationalCenterContext>(options =>
{
    var connString = builder.Configuration.GetConnectionString("OracleDb");
    options.UseOracle(connString);
});

// HEALTH CHECKS (inclui teste do DbContext)
builder.Services.AddHealthChecks()
    .AddDbContextCheck<EducationalCenterContext>("database");

// TRACING
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("EducationCenter.Api"))
    .WithTracing(t =>
    {
        t.AddAspNetCoreInstrumentation();
        t.AddHttpClientInstrumentation();
        t.AddConsoleExporter();
    });

// MVC + API VERSIONING
builder.Services.AddControllers();

builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ReportApiVersions = true;
});

builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";      // v1, v1.0, etc
    options.SubstituteApiVersionInUrl = true;
});

// SWAGGER
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

// HEALTH ENDPOINTS
app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready");

app.Run();
