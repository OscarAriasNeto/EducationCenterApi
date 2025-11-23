using EducationCenter.Data;
using EducationCenter.DTOs;
using EducationCenter.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EducationCenter.Controllers.v2;

[ApiController]
[ApiVersion("2.0")]
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

    // v2: suporta filtros por nome e profissão
    [HttpGet(Name = "GetStudentsV2")]
    public async Task<ActionResult<PagedResponse<StudentDto>>> GetAll(
        string? name = null,
        int? professionId = null,
        int pageNumber = 1,
        int pageSize = 10)
    {
        _logger.LogInformation("Listando alunos (v2) com filtros {@name} {@professionId}", name, professionId);

        if (pageNumber <= 0 || pageSize <= 0)
            return BadRequest("pageNumber e pageSize devem ser maiores que zero.");

        var query = _context.Students
            .Include(s => s.TargetProfession)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(name))
        {
            var upper = name.ToUpperInvariant();
            query = query.Where(s => s.FullName.ToUpper().Contains(upper));
        }

        if (professionId.HasValue)
        {
            query = query.Where(s => s.TargetProfessionId == professionId.Value);
        }

        var totalItems = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        var data = await query
            .OrderBy(s => s.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = data.Select(s => s.ToDto()).ToList();

        var response = new PagedResponse<StudentDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalPages,
            Links = new[]
            {
                new LinkDto
                {
                    Href = Url.Link("GetStudentsV2", new { pageNumber, pageSize, name, professionId, version = "2.0" })!,
                    Rel = "self",
                    Method = "GET"
                }
            }
        };

        return Ok(response);
    }

    [HttpGet("{id:int}", Name = "GetStudentByIdV2")]
    public async Task<ActionResult<Resource<StudentDetailDto>>> GetById(int id)
    {
        _logger.LogInformation("Buscando aluno por ID (v2) {@id}", id);

        var student = await _context.Students
            .Include(s => s.TargetProfession)
            .Include(s => s.StudentLearningPaths)
                .ThenInclude(sl => sl.LearningPath)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);

        if (student == null)
            return NotFound();

        var resource = new Resource<StudentDetailDto>
        {
            Data = student.ToDetailDto(),
            Links = new[]
            {
                new LinkDto
                {
                    Href = Url.Link("GetStudentByIdV2", new { id, version = "2.0" })!,
                    Rel = "self",
                    Method = "GET"
                },
                new LinkDto
                {
                    Href = Url.Link("GetStudentsV2", new { pageNumber = 1, pageSize = 10, version = "2.0" })!,
                    Rel = "students",
                    Method = "GET"
                }
            }
        };

        return Ok(resource);
    }

    [HttpPost]
    public async Task<ActionResult<Resource<StudentDetailDto>>> Create(StudentCreateDto dto)
    {
        var entity = dto.FromCreateDto();

        _context.Students.Add(entity);
        await _context.SaveChangesAsync();

        var detail = entity.ToDetailDto();

        var resource = new Resource<StudentDetailDto>
        {
            Data = detail,
            Links = new[]
            {
                new LinkDto
                {
                    Href = Url.Link("GetStudentByIdV2", new { id = entity.Id, version = "2.0" })!,
                    Rel = "self",
                    Method = "GET"
                }
            }
        };

        return CreatedAtRoute(
            routeName: "GetStudentByIdV2",
            routeValues: new { id = entity.Id, version = "2.0" },
            value: resource
        );
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, StudentUpdateDto dto)
    {
        var entity = await _context.Students.FindAsync(id);
        if (entity == null) return NotFound();

        entity.UpdateFromDto(dto);
        await _context.SaveChangesAsync();

        return NoContent();
    }

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
