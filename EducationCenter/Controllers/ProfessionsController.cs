using EducationalCenter.Api.Data;
using EducationalCenter.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EducationalCenter.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProfessionsController : ControllerBase
{
    private readonly EducationalCenterContext _context;

    public ProfessionsController(EducationalCenterContext context)
    {
        _context = context;
    }

    // GET api/professions
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Profession>>> GetAll()
    {
        return await _context.Professions.ToListAsync();
    }

    // GET api/professions/1
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Profession>> GetById(int id)
    {
        var profession = await _context.Professions
            .Include(p => p.LearningPaths)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (profession == null) return NotFound();

        return profession;
    }
}
