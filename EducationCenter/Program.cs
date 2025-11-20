using EducationalCenter.Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// EF Core (InMemory pra testar rápido)
builder.Services.AddDbContext<EducationalCenterContext>(options =>
    options.UseInMemoryDatabase("EducationalCenterDb"));

// Se quiser SQL Server, depois trocamos pra UseSqlServer

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

app.Run();
