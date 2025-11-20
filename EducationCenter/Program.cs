using EducationCenter.Data;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// ========== LOGGING ==========
builder.Logging.ClearProviders();      // limpa provedores padrões
builder.Logging.AddConsole();          // log no console (bom pra debug/docker)

// ========== DB CONTEXT ==========
builder.Services.AddDbContext<EducationalCenterContext>(options =>
    options.UseInMemoryDatabase("EducationalCenterDb"));
// se depois trocar pra SQL Server, só trocar o UseInMemoryDatabase por UseSqlServer

// ========== HEALTH CHECKS ==========
builder.Services.AddHealthChecks()
    // health check básico do contexto de banco (InMemory vai dar sempre healthy;
    // se usar SQL Server, ele vai testar conexão)
    .AddDbContextCheck<EducationalCenterContext>("database");

// ========== TRACING (OpenTelemetry) ==========
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource =>
        resource.AddService("EducationCenter.Api")) // nome do serviço para observabilidade
    .WithTracing(tracing =>
    {
        tracing.AddAspNetCoreInstrumentation(); // requests HTTP da API
        tracing.AddHttpClientInstrumentation(); // chamadas HttpClient
        // se usar SQL Server, pode futuramente adicionar: tracing.AddSqlClientInstrumentation();
        tracing.AddConsoleExporter();           // envia os traces para o console
    });

builder.Services.AddControllers();
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

// ========== ENDPOINTS DE HEALTH CHECK ==========

// Liveness: só diz se a app está de pé
app.MapHealthChecks("/health/live");

// Readiness: dá um JSON com status dos checks (incluindo banco)
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";

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

        await context.Response.WriteAsync(JsonSerializer.Serialize(result));
    }
});

app.Run();
