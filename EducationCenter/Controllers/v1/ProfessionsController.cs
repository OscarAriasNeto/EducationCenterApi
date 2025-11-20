using EducationCenter.Data;
using EducationCenter.DTOs;
using EducationCenter.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EducationCenter.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/professions")]
public class ProfessionsController : ControllerBase
{
    private readonly EducationalCenterContext _context;

    public ProfessionsController(EducationalCenterContext context)
    {
        _context = context;
    }

    [HttpGet(Name = "GetProfessionsV1")]
    public async Task<ActionResult<PagedResponse<ProfessionDto>>> GetAll(int pageNumber = 1, int pageSize = 10)
    {
        var query = _context.Professions.AsNoTracking();

        var total = await query.CountAsync();
        var pages = (int)Math.Ceiling(total / (double)pageSize);

        var data = await query
            .OrderBy(x => x.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new PagedResponse<ProfessionDto>
        {
            Items = data.Select(p => p.ToDto()).ToList(),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = total,
            TotalPages = pages
        });
    }

    [HttpGet("{id:int}", Name = "GetProfessionByIdV1")]
    public async Task<ActionResult<Resource<ProfessionDetailDto>>> GetById(int id)
    {
        var profession = await _context.Professions
            .Include(p => p.LearningPaths)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (profession == null) return NotFound();

        return Ok(new Resource<ProfessionDetailDto>
        {
            Data = profession.ToDetailDto()
        });
    }

    [HttpPost]
    public async Task<ActionResult<Resource<ProfessionDetailDto>>> Create(ProfessionCreateDto dto)
    {
        var entity = dto.FromCreateDto();
        _context.Professions.Add(entity);
        await _context.SaveChangesAsync();

        return CreatedAtRoute(
            "GetProfessionByIdV1",
            new { id = entity.Id, version = "1" },
            new Resource<ProfessionDetailDto> { Data = entity.ToDetailDto() }
        );
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, ProfessionUpdateDto dto)
    {
        var entity = await _context.Professions.FindAsync(id);
        if (entity == null) return NotFound();

        entity.UpdateFromDto(dto);
        await _context.SaveChangesAsync();

        return NoContent();
    }

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
