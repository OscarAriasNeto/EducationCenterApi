using EducationCenter.Data;
using EducationCenter.DTOs;
using EducationCenter.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EducationCenter.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProfessionsController : ControllerBase
{
    private readonly EducationalCenterContext _context;
    private readonly ILogger<ProfessionsController> _logger;

    public ProfessionsController(
        EducationalCenterContext context,
        ILogger<ProfessionsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET api/professions?pageNumber=1&pageSize=10
    [HttpGet(Name = "GetProfessions")]
    [ProducesResponseType(typeof(PagedResponse<ProfessionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<ProfessionDto>>> GetAll(
        int pageNumber = 1,
        int pageSize = 10)
    {
        _logger.LogInformation("GET /professions page={Page} size={Size}", pageNumber, pageSize);

        if (pageNumber <= 0 || pageSize <= 0)
        {
            _logger.LogWarning("Parâmetros inválidos: pageNumber={Page}, pageSize={Size}", pageNumber, pageSize);
            return BadRequest("Os parâmetros de paginação devem ser maiores que zero.");
        }

        var query = _context.Professions.AsNoTracking();

        var totalItems = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        var items = await query
            .OrderBy(p => p.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(p => p.ToDto())
            .ToListAsync();

        var response = new PagedResponse<ProfessionDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalPages,
            Links = BuildProfessionsCollectionLinks(pageNumber, pageSize, totalPages)
        };

        return Ok(response);
    }

    // GET api/professions/1
    [HttpGet("{id:int}", Name = "GetProfessionById")]
    [ProducesResponseType(typeof(Resource<ProfessionDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Resource<ProfessionDetailDto>>> GetById(int id)
    {
        _logger.LogInformation("GET /professions/{Id}", id);

        var profession = await _context.Professions
            .Include(p => p.LearningPaths)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (profession == null)
        {
            _logger.LogWarning("Profession {Id} não encontrada.", id);
            return NotFound();
        }

        var resource = new Resource<ProfessionDetailDto>
        {
            Data = profession.ToDetailDto(),
            Links = BuildProfessionLinks(id)
        };

        return Ok(resource);
    }

    // POST api/professions
    [HttpPost]
    [ProducesResponseType(typeof(Resource<ProfessionDetailDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Resource<ProfessionDetailDto>>> Create([FromBody] ProfessionCreateDto dto)
    {
        _logger.LogInformation("POST /professions criando profissão {Name}", dto.Name);

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var entity = dto.FromCreateDto();
        _context.Professions.Add(entity);
        await _context.SaveChangesAsync();

        var created = await _context.Professions
            .Include(p => p.LearningPaths)
            .AsNoTracking()
            .FirstAsync(p => p.Id == entity.Id);

        var resource = new Resource<ProfessionDetailDto>
        {
            Data = created.ToDetailDto(),
            Links = BuildProfessionLinks(entity.Id)
        };

        return CreatedAtRoute("GetProfessionById", new { id = entity.Id }, resource);
    }

    // PUT api/professions/1
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] ProfessionUpdateDto dto)
    {
        _logger.LogInformation("PUT /professions/{Id}", id);

        var entity = await _context.Professions.FindAsync(id);
        if (entity == null)
        {
            _logger.LogWarning("Tentativa de editar profissão inexistente {Id}", id);
            return NotFound();
        }

        entity.UpdateFromDto(dto);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE api/professions/1
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        _logger.LogInformation("DELETE /professions/{Id}", id);

        var entity = await _context.Professions.FindAsync(id);
        if (entity == null)
            return NotFound();

        _context.Professions.Remove(entity);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // ===== HATEOAS =====
    private IEnumerable<LinkDto> BuildProfessionLinks(int id)
    {
        return new List<LinkDto>
        {
            new() { Href = Url.Link("GetProfessionById", new { id })!, Rel = "self", Method = "GET" },
            new() { Href = Url.Link("GetProfessions", new { pageNumber = 1, pageSize = 10 })!, Rel = "all", Method = "GET" },
            new() { Href = Url.Action(nameof(Update), new { id })!, Rel = "update", Method = "PUT" },
            new() { Href = Url.Action(nameof(Delete), new { id })!, Rel = "delete", Method = "DELETE" }
        };
    }

    private IEnumerable<LinkDto> BuildProfessionsCollectionLinks(int pageNumber, int pageSize, int totalPages)
    {
        var links = new List<LinkDto>
        {
            new() { Href = Url.Link("GetProfessions", new { pageNumber, pageSize })!, Rel = "self", Method = "GET" }
        };

        if (pageNumber > 1)
            links.Add(new LinkDto { Href = Url.Link("GetProfessions", new { pageNumber = pageNumber - 1, pageSize })!, Rel = "prev", Method = "GET" });

        if (pageNumber < totalPages)
            links.Add(new LinkDto { Href = Url.Link("GetProfessions", new { pageNumber = pageNumber + 1, pageSize })!, Rel = "next", Method = "GET" });

        return links;
    }
}
