using EducationCenter.Data;
using EducationCenter.DTOs;
using EducationCenter.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EducationCenter.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/students")]
public class StudentsController : ControllerBase
{
    private readonly EducationalCenterContext _context;
    private readonly ILogger<StudentsController> _logger;

    public StudentsController(EducationalCenterContext context, ILogger<StudentsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // =======================================
    // GET ALL (paginação + HATEOAS)
    // =======================================
    [HttpGet(Name = "GetStudentsV1")]
    public async Task<ActionResult<PagedResponse<StudentDto>>> GetAll(
        int pageNumber = 1, int pageSize = 10)
    {
        _logger.LogInformation("Listando alunos (v1)");

        var query = _context.Students
            .Include(s => s.TargetProfession)
            .AsNoTracking();

        var totalItems = await query.CountAsync();
        var pages = (int)Math.Ceiling(totalItems / (double)pageSize);

        var data = await query
            .OrderBy(s => s.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = data.Select(s => s.ToDto()).ToList();

        return Ok(new PagedResponse<StudentDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = pages,
            Links = new[]
            {
                new LinkDto { Href = Url.Link("GetStudentsV1", new { pageNumber, pageSize, version = "1" })!, Rel="self", Method="GET" }
            }
        });
    }

    // =======================================
    // GET BY ID
    // =======================================
    [HttpGet("{id:int}", Name = "GetStudentByIdV1")]
    public async Task<ActionResult<Resource<StudentDetailDto>>> GetById(int id)
    {
        _logger.LogInformation("Buscando aluno por ID (v1)");

        var student = await _context.Students
            .Include(s => s.TargetProfession)
            .Include(s => s.StudentLearningPaths)
                .ThenInclude(lp => lp.LearningPath)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);

        if (student == null)
            return NotFound();

        return Ok(new Resource<StudentDetailDto>
        {
            Data = student.ToDetailDto(),
            Links = new[]
            {
                new LinkDto{ Href = Url.Link("GetStudentByIdV1", new { id, version="1" })!, Rel="self", Method="GET"},
                new LinkDto{ Href = Url.Link("GetStudentsV1", new { pageNumber=1, pageSize=10, version="1" })!, Rel="students", Method="GET"}
            }
        });
    }

    // =======================================
    // CREATE
    // =======================================
    [HttpPost]
    public async Task<ActionResult<Resource<StudentDetailDto>>> Create(StudentCreateDto dto)
    {
        var entity = dto.FromCreateDto();

        _context.Students.Add(entity);
        await _context.SaveChangesAsync();

        return CreatedAtRoute(
            "GetStudentByIdV1",
            new { id = entity.Id, version = "1" },
            new Resource<StudentDetailDto>
            {
                Data = entity.ToDetailDto(),
                Links = Array.Empty<LinkDto>()
            });
    }

    // =======================================
    // UPDATE
    // =======================================
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, StudentUpdateDto dto)
    {
        var entity = await _context.Students.FindAsync(id);
        if (entity == null) return NotFound();

        entity.UpdateFromDto(dto);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // =======================================
    // DELETE
    // =======================================
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _context.Students
            .Include(s => s.StudentLearningPaths)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (entity == null) return NotFound();

        _context.StudentLearningPaths.RemoveRange(entity.StudentLearningPaths);
        _context.Students.Remove(entity);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
