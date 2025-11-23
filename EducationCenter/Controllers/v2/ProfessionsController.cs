using EducationCenter.Data;
using EducationCenter.DTOs;
using EducationCenter.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EducationCenter.Controllers.v2;

[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/professions")]
public class ProfessionsController : ControllerBase
{
    private readonly EducationalCenterContext _context;
    private readonly ILogger<ProfessionsController> _logger;

    public ProfessionsController(EducationalCenterContext context, ILogger<ProfessionsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet(Name = "GetProfessionsV2")]
    public async Task<ActionResult<PagedResponse<ProfessionDto>>> GetAll(
        string? search = null,
        int pageNumber = 1,
        int pageSize = 10)
    {
        if (pageNumber <= 0 || pageSize <= 0)
            return BadRequest("pageNumber e pageSize devem ser maiores que zero.");

        var query = _context.Professions.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var upper = search.ToUpperInvariant();
            query = query.Where(p =>
                p.Name.ToUpper().Contains(upper) ||
                p.Description.ToUpper().Contains(upper));
        }

        var totalItems = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        var data = await query
            .OrderBy(p => p.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = data.Select(p => p.ToDto()).ToList();

        return Ok(new PagedResponse<ProfessionDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalPages
        });
    }

    [HttpGet("{id:int}", Name = "GetProfessionByIdV2")]
    public async Task<ActionResult<Resource<ProfessionDetailDto>>> GetById(int id)
    {
        var profession = await _context.Professions
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

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

        var created = await _context.Professions
            .AsNoTracking()
            .FirstAsync(p => p.Id == entity.Id);

        return CreatedAtRoute(
            "GetProfessionByIdV2",
            new { id = created.Id, version = "2.0" },
            new Resource<ProfessionDetailDto> { Data = created.ToDetailDto() }
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
