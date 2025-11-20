using EducationCenter.Data;
using EducationCenter.DTOs;
using EducationCenter.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
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

    // GET api/professions?pageNumber=1&pageSize=10
    [HttpGet(Name = "GetProfessions")]
    [ProducesResponseType(typeof(PagedResponse<ProfessionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<ProfessionDto>>> GetAll(
        int pageNumber = 1,
        int pageSize = 10)
    {
        if (pageNumber <= 0 || pageSize <= 0)
            return BadRequest("pageNumber e pageSize devem ser maiores que zero.");

        var query = _context.Professions.AsNoTracking();

        var totalItems = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        var entities = await query
            .OrderBy(p => p.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = entities.Select(p => p.ToDto()).ToList();

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
        var profession = await _context.Professions
            .Include(p => p.LearningPaths)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (profession == null) return NotFound();

        var dto = profession.ToDetailDto();

        var resource = new Resource<ProfessionDetailDto>
        {
            Data = dto,
            Links = BuildProfessionLinks(id)
        };

        return Ok(resource);
    }

    // POST api/professions
    [HttpPost]
    [ProducesResponseType(typeof(Resource<ProfessionDetailDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Resource<ProfessionDetailDto>>> Create(
        [FromBody] ProfessionCreateDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var entity = dto.FromCreateDto();
        _context.Professions.Add(entity);
        await _context.SaveChangesAsync();

        var created = await _context.Professions
            .Include(p => p.LearningPaths)
            .AsNoTracking()
            .FirstAsync(p => p.Id == entity.Id);

        var detailDto = created.ToDetailDto();

        var resource = new Resource<ProfessionDetailDto>
        {
            Data = detailDto,
            Links = BuildProfessionLinks(created.Id)
        };

        return CreatedAtRoute(
            routeName: "GetProfessionById",
            routeValues: new { id = created.Id },
            value: resource
        );
    }

    // PUT api/professions/1
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, [FromBody] ProfessionUpdateDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var entity = await _context.Professions.FindAsync(id);
        if (entity == null) return NotFound();

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
        var entity = await _context.Professions.FindAsync(id);
        if (entity == null) return NotFound();

        _context.Professions.Remove(entity);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // ===== HATEOAS helpers =====

    private IEnumerable<LinkDto> BuildProfessionLinks(int id)
    {
        var links = new List<LinkDto>
        {
            new()
            {
                Href = Url.Link("GetProfessionById", new { id })!,
                Rel = "self",
                Method = "GET"
            },
            new()
            {
                Href = Url.Link("GetProfessions", new { pageNumber = 1, pageSize = 10 })!,
                Rel = "professions",
                Method = "GET"
            },
            new()
            {
                Href = Url.Action(nameof(Update), values: new { id })!,
                Rel = "update_profession",
                Method = "PUT"
            },
            new()
            {
                Href = Url.Action(nameof(Delete), values: new { id })!,
                Rel = "delete_profession",
                Method = "DELETE"
            }
        };

        return links;
    }

    private IEnumerable<LinkDto> BuildProfessionsCollectionLinks(int pageNumber, int pageSize, int totalPages)
    {
        var links = new List<LinkDto>
        {
            new()
            {
                Href = Url.Link("GetProfessions", new { pageNumber, pageSize })!,
                Rel = "self",
                Method = "GET"
            }
        };

        if (pageNumber > 1)
        {
            links.Add(new LinkDto
            {
                Href = Url.Link("GetProfessions", new { pageNumber = pageNumber - 1, pageSize })!,
                Rel = "prev_page",
                Method = "GET"
            });
        }

        if (pageNumber < totalPages)
        {
            links.Add(new LinkDto
            {
                Href = Url.Link("GetProfessions", new { pageNumber = pageNumber + 1, pageSize })!,
                Rel = "next_page",
                Method = "GET"
            });
        }

        return links;
    }
}
