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

namespace EducationCenter.Tests.LearningPathsV2;

public class LearningPathsControllerV2Tests
{
    private EducationalCenterContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<EducationalCenterContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new EducationalCenterContext(options);

        var profession = new Profession
        {
            Id = 1,
            Name = "Dev Back-end",
            Description = "teste",
            MarketOverview = "alta demanda"
        };

        context.Professions.Add(profession);

        context.LearningPaths.Add(new LearningPath
        {
            Id = 1,
            Title = "Trilha Back-end",
            Description = "descrição",
            ProfessionId = 1
        });

        context.SaveChanges();
        return context;
    }

    // >>> sempre usa este helper para criar o controller <<<
    private LearningPathsController CreateController(EducationalCenterContext context)
    {
        var loggerMock = new Mock<ILogger<LearningPathsController>>();
        return new LearningPathsController(context, loggerMock.Object);
    }

    [Fact]
    public async Task GetById_DeveRetornarOk_QuandoLearningPathExiste_V2()
    {
        var context = CreateContext();
        var controller = CreateController(context);

        var result = await controller.GetById(1);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var resource = Assert.IsType<Resource<LearningPathDetailDto>>(ok.Value);
        Assert.Equal(1, resource.Data.Id);
        Assert.Equal("Trilha Back-end", resource.Data.Title);
    }

    [Fact]
    public async Task GetById_DeveRetornarNotFound_QuandoLearningPathNaoExiste_V2()
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
        var paged = Assert.IsType<PagedResponse<LearningPathSummaryDto>>(ok.Value);

        Assert.Equal(1, paged.TotalItems);
        Assert.Single(paged.Items);
    }
}
