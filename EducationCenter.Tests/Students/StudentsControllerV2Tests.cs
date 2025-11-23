using System;
using System.Linq;
using System.Threading.Tasks;
using EducationCenter.Controllers.v2;
using EducationCenter.Data;
using EducationCenter.DTOs;
using EducationCenter.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace EducationCenter.Tests.StudentsV2;

public class StudentsControllerV2Tests
{
    private EducationalCenterContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<EducationalCenterContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new EducationalCenterContext(options);

        var dev = new Profession
        {
            Id = 1,
            Name = "Dev Back-end",
            Description = "teste",
            MarketOverview = "alta demanda"
        };

        var qa = new Profession
        {
            Id = 2,
            Name = "QA",
            Description = "teste",
            MarketOverview = "em crescimento"
        };

        context.Professions.AddRange(dev, qa);

        context.Students.AddRange(
            new Student
            {
                Id = 1,
                FullName = "Aluno Teste",
                Email = "aluno@teste.com",
                BirthDate = new DateTime(2000, 1, 1),
                TargetProfessionId = 1
            },
            new Student
            {
                Id = 2,
                FullName = "Outro Aluno",
                Email = "outro@teste.com",
                BirthDate = new DateTime(1999, 5, 10),
                TargetProfessionId = 2
            }
        );

        context.SaveChanges();
        return context;
    }

    private StudentsController CreateController(EducationalCenterContext context)
    {
        var loggerMock = new Mock<ILogger<StudentsController>>();
        var controller = new StudentsController(context, loggerMock.Object);

        // Configurar HttpContext e UrlHelper para HATEOAS não quebrar
        var httpContext = new DefaultHttpContext();
        var actionContext = new ActionContext(
            httpContext,
            new RouteData(),
            new ControllerActionDescriptor());

        controller.ControllerContext = new ControllerContext(actionContext);

        var urlHelperMock = new Mock<IUrlHelper>();
        urlHelperMock
            .Setup(u => u.Link(It.IsAny<string>(), It.IsAny<object>()))
            .Returns("http://localhost/test-link");
        urlHelperMock
            .Setup(u => u.Action(It.IsAny<UrlActionContext>()))
            .Returns("http://localhost/test-action");

        controller.Url = urlHelperMock.Object;
        return controller;
    }

    [Fact]
    public async Task GetById_DeveRetornarOk_QuandoAlunoExiste_V2()
    {
        var context = CreateContext();
        var controller = CreateController(context);

        var result = await controller.GetById(1);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var resource = Assert.IsType<Resource<StudentDetailDto>>(okResult.Value);
        Assert.Equal(1, resource.Data.Id);
        Assert.Equal("Aluno Teste", resource.Data.FullName);
    }

    [Fact]
    public async Task GetById_DeveRetornarNotFound_QuandoAlunoNaoExiste_V2()
    {
        var context = CreateContext();
        var controller = CreateController(context);

        var result = await controller.GetById(999);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetAll_ComFiltro_DeveRetornarListaFiltrada_V2()
    {
        var context = CreateContext();
        var controller = CreateController(context);

        var result = await controller.GetAll(
            name: "Aluno",
            professionId: 1,
            pageNumber: 1,
            pageSize: 10);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var paged = Assert.IsType<PagedResponse<StudentDto>>(okResult.Value);

        Assert.Equal(1, paged.TotalItems);
        Assert.Single(paged.Items);
        Assert.Equal("Aluno Teste", paged.Items.First().FullName);
    }
}
