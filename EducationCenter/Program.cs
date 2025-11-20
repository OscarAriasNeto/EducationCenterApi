using EducationCenter.Data;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Versioning;

var builder = WebApplication.CreateBuilder(args);

// =======================================================================
// LOGGING
// =======================================================================
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// =======================================================================
// DATABASE (InMemory)
// =======================================================================
builder.Services.AddDbContext<EducationalCenterContext>(options =>
    options.UseInMemoryDatabase("EducationalCenterDb"));

// =======================================================================
// API VERSIONING
// =======================================================================
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;

    // versão via URL → /api/v1/students
    options.ApiVersionReader = new UrlSegmentApiVersionReader();

    // retorna "api-supported-versions" no header
    options.ReportApiVersions = true;
});

// Permite que o Swagger enxergue múltiplas versões
builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV"; // v1, v2
    options.SubstituteApiVersionInUrl = true;
});

// =======================================================================
// HEALTH CHECKS
// =======================================================================
builder.Services.AddHealthChecks()
    .AddDbContextCheck<EducationalCenterContext>("database");

// =======================================================================
// TRACING (OpenTelemetry)
// =======================================================================
builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService("EducationCenter.Api"))
    .WithTracing(t =>
    {
        t.AddAspNetCoreInstrumentation();
        t.AddHttpClientInstrumentation();
        t.AddConsoleExporter();
    });

// =======================================================================
// CONTROLLERS + SWAGGER
// =======================================================================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// =======================================================================
// SWAGGER
// =======================================================================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// =======================================================================
// MIDDLEWARES
// =======================================================================
app.UseHttpsRedirection();

// =======================================================================
// HEALTH CHECK ENDPOINTS
// =======================================================================
app.MapHealthChecks("/health/live");

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = async (ctx, report) =>
    {
        ctx.Response.ContentType = "application/json";

        var result = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description
            })
        };

        await ctx.Response.WriteAsync(JsonSerializer.Serialize(result));
    }
});

app.MapControllers();

app.Run();
