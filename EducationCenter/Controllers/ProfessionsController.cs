using EducationCenter.Data;
using EducationCenter.DTOs;
using EducationCenter.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EducationCenter.Controllers;


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

    // POST api/professions
    [HttpPost]
    public async Task<ActionResult<ProfessionDetailDto>> Create([FromBody] ProfessionCreateDto dto)
    {
        var entity = dto.FromCreateDto();
        _context.Professions.Add(entity);
        await _context.SaveChangesAsync();

        // recarrega com include para devolver detailDto
        var created = await _context.Professions
            .Include(p => p.LearningPaths)
            .FirstAsync(p => p.Id == entity.Id);

        return CreatedAtAction(
            nameof(GetById),
            new { id = created.Id },
            created.ToDetailDto()
        );
    }

    // PUT api/professions/1
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] ProfessionUpdateDto dto)
    {
        var entity = await _context.Professions.FindAsync(id);
        if (entity == null) return NotFound();

        entity.UpdateFromDto(dto);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE api/professions/1
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _context.Professions.FindAsync(id);
        if (entity == null) return NotFound();

        _context.Professions.Remove(entity);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
