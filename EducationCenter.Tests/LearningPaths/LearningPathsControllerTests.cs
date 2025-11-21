using EducationCenter.Controllers;
using EducationCenter.Controllers.v1;
using EducationCenter.Data;
using EducationCenter.DTOs;
using EducationCenter.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace EducationCenter.Tests.LearningPaths;

public class LearningPathsControllerTests
{
    private LearningPathsController GetController()
    {
        var options = new DbContextOptionsBuilder<EducationalCenterContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var ctx = new EducationalCenterContext(options);
        // var logger = Mock.Of<ILogger<LearningPathsController>>(); // Remova esta linha

        return new LearningPathsController(ctx); // Passe apenas o contexto
    }

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        var controller = GetController();

        var result = await controller.GetAll(1, 10);

        Assert.IsType<OkObjectResult>(result.Result);
    }
}
