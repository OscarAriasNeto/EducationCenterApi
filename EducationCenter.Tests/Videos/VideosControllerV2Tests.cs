using System;
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
using Moq;
using Xunit;

namespace EducationCenter.Tests.VideosV2;

public class VideosControllerV2Tests
{
    private EducationalCenterContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<EducationalCenterContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new EducationalCenterContext(options);

        context.Videos.Add(new Video
        {
            Id = 1,
            Title = "Vídeo Teste",
            Description = "descrição",
            Url = "https://example.com",
            DurationMinutes = 10
        });

        context.SaveChanges();
        return context;
    }

    private VideosController CreateController(EducationalCenterContext context)
    {
        var controller = new VideosController(context);

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
    public async Task GetById_DeveRetornarOk_QuandoVideoExiste_V2()
    {
        var context = CreateContext();
        var controller = CreateController(context);

        var result = await controller.GetById(1);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var resource = Assert.IsType<Resource<VideoDto>>(ok.Value);
        Assert.Equal(1, resource.Data.Id);
        Assert.Equal("Vídeo Teste", resource.Data.Title);
    }

    [Fact]
    public async Task GetById_DeveRetornarNotFound_QuandoVideoNaoExiste_V2()
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
        var paged = Assert.IsType<PagedResponse<VideoDto>>(ok.Value);

        Assert.Equal(1, paged.TotalItems);
        Assert.Single(paged.Items);
    }
}
