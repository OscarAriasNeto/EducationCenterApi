using EducationCenter.Data;
using EducationCenter.DTOs;
using EducationCenter.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EducationCenter.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StudentsController : ControllerBase
{
    private readonly EducationalCenterContext _context;
    private readonly ILogger<StudentsController> _logger;

    public StudentsController(
        EducationalCenterContext context,
        ILogger<StudentsController> logger)
    {
        _context = context;
        _logger = logger;
    }


    // GET api/students?pageNumber=1&pageSize=10
    [HttpGet(Name = "GetStudents")]
    [ProducesResponseType(typeof(PagedResponse<StudentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<StudentDto>>> GetAll(
        int pageNumber = 1,
        int pageSize = 10)
    {
        if (pageNumber <= 0 || pageSize <= 0)
            return BadRequest("pageNumber e pageSize devem ser maiores que zero.");

        var query = _context.Students
            .Include(s => s.TargetProfession)
            .AsNoTracking();

        var totalItems = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        var entities = await query
            .OrderBy(s => s.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = entities.Select(s => s.ToDto()).ToList();

        var response = new PagedResponse<StudentDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalPages,
            Links = BuildStudentsCollectionLinks(pageNumber, pageSize, totalPages)
        };

        return Ok(response);
    }

    // GET api/students/1
    [HttpGet("{id:int}", Name = "GetStudentById")]
    [ProducesResponseType(typeof(Resource<StudentDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Resource<StudentDetailDto>>> GetById(int id)
    {
        var student = await _context.Students
            .Include(s => s.TargetProfession)
            .Include(s => s.StudentLearningPaths)
                .ThenInclude(sl => sl.LearningPath)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);

        if (student == null) return NotFound();

        var dto = student.ToDetailDto();

        var resource = new Resource<StudentDetailDto>
        {
            Data = dto,
            Links = BuildStudentLinks(id)
        };

        return Ok(resource);
    }

    // POST api/students
    [HttpPost]
    [ProducesResponseType(typeof(Resource<StudentDetailDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Resource<StudentDetailDto>>> Create([FromBody] StudentCreateDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var entity = dto.FromCreateDto();
        _context.Students.Add(entity);
        await _context.SaveChangesAsync();

        var created = await _context.Students
            .Include(s => s.TargetProfession)
            .Include(s => s.StudentLearningPaths)
                .ThenInclude(sl => sl.LearningPath)
            .AsNoTracking()
            .FirstAsync(s => s.Id == entity.Id);

        var detailDto = created.ToDetailDto();

        var resource = new Resource<StudentDetailDto>
        {
            Data = detailDto,
            Links = BuildStudentLinks(created.Id)
        };

        return CreatedAtRoute(
            routeName: "GetStudentById",
            routeValues: new { id = created.Id },
            value: resource
        );
    }

    // PUT api/students/1
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, [FromBody] StudentUpdateDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var entity = await _context.Students.FindAsync(id);
        if (entity == null) return NotFound();

        entity.UpdateFromDto(dto);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE api/students/1
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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

    // ===== Métodos privados para montar HATEOAS =====

    private IEnumerable<LinkDto> BuildStudentLinks(int id)
    {
        var links = new List<LinkDto>
        {
            new()
            {
                Href = Url.Link("GetStudentById", new { id })!,
                Rel = "self",
                Method = "GET"
            },
            new()
            {
                Href = Url.Link("GetStudents", new { pageNumber = 1, pageSize = 10 })!,
                Rel = "students",
                Method = "GET"
            },
            new()
            {
                Href = Url.Link(null, new { }) ?? Url.Action(nameof(Update), values: new { id })!,
                Rel = "update_student",
                Method = "PUT"
            },
            new()
            {
                Href = Url.Action(nameof(Delete), values: new { id })!,
                Rel = "delete_student",
                Method = "DELETE"
            }
        };

        return links;
    }

    private IEnumerable<LinkDto> BuildStudentsCollectionLinks(int pageNumber, int pageSize, int totalPages)
    {
        var links = new List<LinkDto>
        {
            new()
            {
                Href = Url.Link("GetStudents", new { pageNumber, pageSize })!,
                Rel = "self",
                Method = "GET"
            }
        };

        if (pageNumber > 1)
        {
            links.Add(new LinkDto
            {
                Href = Url.Link("GetStudents", new { pageNumber = pageNumber - 1, pageSize })!,
                Rel = "prev_page",
                Method = "GET"
            });
        }

        if (pageNumber < totalPages)
        {
            links.Add(new LinkDto
            {
                Href = Url.Link("GetStudents", new { pageNumber = pageNumber + 1, pageSize })!,
                Rel = "next_page",
                Method = "GET"
            });
        }

        return links;
    }
}
