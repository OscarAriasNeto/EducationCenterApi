using EducationCenter.Data;
using EducationCenter.DTOs;
using EducationCenter.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EducationCenter.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StudentsController : ControllerBase
{
    private readonly EducationalCenterContext _context;

    public StudentsController(EducationalCenterContext context)
    {
        _context = context;
    }

    // GET api/students
    [HttpGet]
    public async Task<ActionResult<IEnumerable<StudentDto>>> GetAll()
    {
        var entities = await _context.Students
            .Include(s => s.TargetProfession)
            .ToListAsync();

        var dtos = entities.Select(s => s.ToDto()).ToList();
        return Ok(dtos);
    }

    // GET api/students/1
    [HttpGet("{id:int}")]
    public async Task<ActionResult<StudentDetailDto>> GetById(int id)
    {
        var student = await _context.Students
            .Include(s => s.TargetProfession)
            .Include(s => s.StudentLearningPaths)
                .ThenInclude(sl => sl.LearningPath)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (student == null) return NotFound();

        return Ok(student.ToDetailDto());
    }

    // POST api/students
    [HttpPost]
    public async Task<ActionResult<StudentDetailDto>> Create([FromBody] StudentCreateDto dto)
    {
        var entity = dto.FromCreateDto();
        _context.Students.Add(entity);
        await _context.SaveChangesAsync();

        var created = await _context.Students
            .Include(s => s.TargetProfession)
            .Include(s => s.StudentLearningPaths)
                .ThenInclude(sl => sl.LearningPath)
            .FirstAsync(s => s.Id == entity.Id);

        return CreatedAtAction(
            nameof(GetById),
            new { id = created.Id },
            created.ToDetailDto()
        );
    }

    // PUT api/students/1
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] StudentUpdateDto dto)
    {
        var entity = await _context.Students.FindAsync(id);
        if (entity == null) return NotFound();

        entity.UpdateFromDto(dto);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE api/students/1
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
