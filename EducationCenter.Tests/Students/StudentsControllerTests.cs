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

namespace EducationCenter.Tests.Students;

public class StudentsControllerTests
{
    private EducationalCenterContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<EducationalCenterContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new EducationalCenterContext(options);

        // seed básico
        context.Students.Add(new Student
        {
            Id = 1,
            FullName = "Aluno Teste",
            Email = "aluno@teste.com",
            BirthDate = new DateTime(2000, 1, 1)
        });

        context.SaveChanges();
        return context;
    }

    [Fact]
    public async Task GetById_DeveRetornarOk_QuandoAlunoExiste()
    {
        // Arrange
        var context = CreateContext();
        var loggerMock = new Mock<ILogger<StudentsController>>();
        var controller = new StudentsController(context, loggerMock.Object);

        // Act
        var result = await controller.GetById(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var resource = Assert.IsType<Resource<StudentDetailDto>>(okResult.Value);
        Assert.Equal(1, resource.Data.Id);
        Assert.Equal("Aluno Teste", resource.Data.FullName);
    }

    [Fact]
    public async Task GetById_DeveRetornarNotFound_QuandoAlunoNaoExiste()
    {
        var context = CreateContext();
        var loggerMock = new Mock<ILogger<StudentsController>>();
        var controller = new StudentsController(context, loggerMock.Object);

        var result = await controller.GetById(999);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetAll_DeveRetornarPaginado()
    {
        var context = CreateContext();
        var loggerMock = new Mock<ILogger<StudentsController>>();
        var controller = new StudentsController(context, loggerMock.Object);

        var result = await controller.GetAll(pageNumber: 1, pageSize: 10);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var paged = Assert.IsType<PagedResponse<StudentDto>>(okResult.Value);

        Assert.Equal(1, paged.TotalItems);
        Assert.Single(paged.Items);
    }
}
