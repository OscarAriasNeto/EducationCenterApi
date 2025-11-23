using System;
using System.Threading.Tasks;
using EducationCenter.Controllers.v2;
using EducationCenter.Data;
using EducationCenter.DTOs;
using EducationCenter.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace EducationCenter.Tests.ProfessionsV2;

public class ProfessionsControllerV2Tests
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

    // >>> sempre usa este helper para criar o controller <<<
    private ProfessionsController CreateController(EducationalCenterContext context)
    {
        var loggerMock = new Mock<ILogger<ProfessionsController>>();
        return new ProfessionsController(context, loggerMock.Object);
    }

    [Fact]
    public async Task GetById_DeveRetornarOk_QuandoProfissaoExiste_V2()
    {
        var context = CreateContext();
        var controller = CreateController(context);

        var result = await controller.GetById(1);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var resource = Assert.IsType<Resource<ProfessionDetailDto>>(ok.Value);
        Assert.Equal(1, resource.Data.Id);
        Assert.Equal("Dev Back-end", resource.Data.Name);
    }

    [Fact]
    public async Task GetById_DeveRetornarNotFound_QuandoProfissaoNaoExiste_V2()
    {
        var context = CreateContext();
        var controller = CreateController(context);

        var result = await controller.GetById(999);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetAll_DeveRetornarListaPaginada_V2()
    {
        var context = CreateContext();
        var controller = CreateController(context);

        var result = await controller.GetAll();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var paged = Assert.IsType<PagedResponse<ProfessionDto>>(ok.Value);

        Assert.Equal(1, paged.TotalItems);
        Assert.Single(paged.Items);
    }
}
