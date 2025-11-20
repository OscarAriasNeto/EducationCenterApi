using EducationalCenter.Api.Data;
using EducationalCenter.Api.DTOs;
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
    public async Task<ActionResult<IEnumerable<ProfessionDto>>> GetAll()
    {
        var entities = await _context.Professions.ToListAsync();
        var dtos = entities.Select(p => p.ToDto()).ToList();
        return Ok(dtos);
    }

    // GET api/professions/1
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProfessionDetailDto>> GetById(int id)
    {
        var profession = await _context.Professions
            .Include(p => p.LearningPaths)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (profession == null) return NotFound();

        return Ok(profession.ToDetailDto());
    }
}
