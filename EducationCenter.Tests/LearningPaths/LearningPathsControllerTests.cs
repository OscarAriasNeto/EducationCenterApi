using EducationCenter.Controllers;
using EducationCenter.Data;
using EducationCenter.DTOs;
using EducationCenter.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;


public class LearningPathsControllerTests
{
    private LearningPathsController GetController()
    {
        var options = new DbContextOptionsBuilder<EducationalCenterContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var ctx = new EducationalCenterContext(options);
        var logger = Mock.Of<ILogger<LearningPathsController>>();

        return new LearningPathsController(ctx, logger);
    }

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        var controller = GetController();
        var result = await controller.GetAll(1, 10);
        Assert.IsType<OkObjectResult>(result.Result);
    }
}
