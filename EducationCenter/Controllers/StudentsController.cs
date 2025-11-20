using EducationalCenter.Api.Data;
using EducationalCenter.Api.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EducationalCenter.Api.Controllers;

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
}
