using System;
using System.Threading.Tasks;
using EducationCenter.Controllers;
using EducationCenter.Data;
using EducationCenter.DTOs;
using EducationCenter.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace EducationCenter.Tests.Professions;

public class ProfessionsControllerTests
{
    private EducationalCenterContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<EducationalCenterContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new EducationalCenterContext(options);

        context.Professions.Add(new Profession
        {
            Id = 1,
            Name = "Dev Back-end",
            Description = "teste",
            MarketOverview = "alta demanda"
        });

        context.SaveChanges();
        return context;
    }

    [Fact]
    public async Task GetById_DeveRetornarOk_QuandoProfissaoExiste()
    {
        var context = CreateContext();
        var loggerMock = new Mock<ILogger<ProfessionsController>>();
        var controller = new ProfessionsController(context);

        var result = await controller.GetById(1);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var resource = Assert.IsType<Resource<ProfessionDetailDto>>(ok.Value);
        Assert.Equal(1, resource.Data.Id);
        Assert.Equal("Dev Back-end", resource.Data.Name);
    }

    [Fact]
    public async Task GetById_DeveRetornarNotFound_QuandoProfissaoNaoExiste()
    {
        var context = CreateContext();
        var controller = new ProfessionsController(context);

        var result = await controller.GetById(999);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetAll_DeveRetornarListaPaginada()
    {
        var context = CreateContext();
        var controller = new ProfessionsController(context);

        var result = await controller.GetAll();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var paged = Assert.IsType<PagedResponse<ProfessionDto>>(ok.Value);

        Assert.Equal(1, paged.TotalItems);
        Assert.Single(paged.Items);
    }
}
