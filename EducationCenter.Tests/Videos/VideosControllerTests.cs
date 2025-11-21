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

namespace EducationCenter.Tests.Videos;

public class VideosControllerTests
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

    [Fact]
    public async Task GetById_DeveRetornarOk_QuandoVideoExiste()
    {
        var context = CreateContext();
        var loggerMock = new Mock<ILogger<VideosController>>();
        var controller = new VideosController(context);

        var result = await controller.GetById(1);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var resource = Assert.IsType<Resource<VideoDto>>(ok.Value);
        Assert.Equal(1, resource.Data.Id);
        Assert.Equal("Vídeo Teste", resource.Data.Title);
    }

    [Fact]
    public async Task GetById_DeveRetornarNotFound_QuandoVideoNaoExiste()
    {
        var context = CreateContext();
        var controller = new VideosController(context);

        var result = await controller.GetById(999);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetAll_DeveRetornarListaPaginada()
    {
        var context = CreateContext();
        var controller = new VideosController(context);

        var result = await controller.GetAll();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var paged = Assert.IsType<PagedResponse<VideoDto>>(ok.Value);

        Assert.Equal(1, paged.TotalItems);
        Assert.Single(paged.Items);
    }
}
