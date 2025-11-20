using EducationalCenter.Api.Data;
using EducationalCenter.Api.Models;
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
    public async Task<ActionResult<IEnumerable<Student>>> GetAll()
    {
        return await _context.Students
            .Include(s => s.TargetProfession)
            .ToListAsync();
    }

    // GET api/students/1
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Student>> GetById(int id)
    {
        var student = await _context.Students
            .Include(s => s.TargetProfession)
            .Include(s => s.StudentLearningPaths)
                .ThenInclude(sl => sl.LearningPath)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (student == null) return NotFound();

        return student;
    }
}
