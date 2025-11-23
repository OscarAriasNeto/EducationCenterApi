using EducationCenter.Data;
using EducationCenter.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Oracle.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// LOGGING
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// DB CONTEXT (Oracle)
builder.Services.AddDbContext<EducationalCenterContext>(options =>
{
    var connString = builder.Configuration.GetConnectionString("OracleDb")
                     ?? throw new InvalidOperationException("Connection string 'OracleDb' não encontrada.");
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

// MVC + CONTROLLERS
builder.Services.AddControllers();

// ===== GEMINI =====
builder.Services.Configure<GeminiOptions>(builder.Configuration.GetSection("Gemini"));
builder.Services.AddHttpClient<GeminiClient>();

// ===== API VERSIONING =====
builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ReportApiVersions = true;
});

// Para o Swagger conseguir descobrir os grupos (v1, v2, ...)
builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";      // v1, v2, v3...
    options.SubstituteApiVersionInUrl = true;
});

// ===== SWAGGER =====
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    // Gera um documento do Swagger para cada versão da API descoberta
    var provider = builder.Services
        .BuildServiceProvider()
        .GetRequiredService<IApiVersionDescriptionProvider>();

    foreach (var description in provider.ApiVersionDescriptions)
    {
        options.SwaggerDoc(
            description.GroupName,
            new OpenApiInfo
            {
                Title = "EducationCenter",
                Version = description.ApiVersion.ToString()
            });
    }
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        // Cria uma entrada de SwaggerEndpoint para cada versão
        var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

        foreach (var description in provider.ApiVersionDescriptions)
        {
            options.SwaggerEndpoint(
                $"/swagger/{description.GroupName}/swagger.json",
                $"EducationCenter {description.ApiVersion}");
        }

        // opcional: mostrar sempre a última versão primeiro
        // options.DefaultModelsExpandDepth(-1);
    });
}

app.UseHttpsRedirection();

app.MapControllers();

// HEALTH ENDPOINTS
app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready");

app.Run();
