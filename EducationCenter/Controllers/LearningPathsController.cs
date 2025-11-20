using EducationCenter.Data;
using EducationCenter.DTOs;
using EducationCenter.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace EducationCenter.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LearningPathsController : ControllerBase
{
    private readonly EducationalCenterContext _context;

    public LearningPathsController(EducationalCenterContext context)
    {
        _context = context;
    }

    // GET api/learningpaths?pageNumber=1&pageSize=10
    [HttpGet(Name = "GetLearningPaths")]
    [ProducesResponseType(typeof(PagedResponse<LearningPathSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<LearningPathSummaryDto>>> GetAll(
        int pageNumber = 1,
        int pageSize = 10)
    {
        if (pageNumber <= 0 || pageSize <= 0)
            return BadRequest("pageNumber e pageSize devem ser maiores que zero.");

        var query = _context.LearningPaths
            .Include(lp => lp.Profession)
            .AsNoTracking();

        var totalItems = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        var entities = await query
            .OrderBy(lp => lp.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = entities.Select(lp => lp.ToSummaryDto()).ToList();

        var response = new PagedResponse<LearningPathSummaryDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalPages,
            Links = BuildLearningPathsCollectionLinks(pageNumber, pageSize, totalPages)
        };

        return Ok(response);
    }

    // GET api/learningpaths/1
    [HttpGet("{id:int}", Name = "GetLearningPathById")]
    [ProducesResponseType(typeof(Resource<LearningPathDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Resource<LearningPathDetailDto>>> GetById(int id)
    {
        var lp = await _context.LearningPaths
            .Include(l => l.Profession)
            .Include(l => l.LearningPathVideos)
                .ThenInclude(lpv => lpv.Video)
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == id);

        if (lp == null) return NotFound();

        var dto = lp.ToDetailDto();

        var resource = new Resource<LearningPathDetailDto>
        {
            Data = dto,
            Links = BuildLearningPathLinks(id)
        };

        return Ok(resource);
    }

    // POST api/learningpaths
    [HttpPost]
    [ProducesResponseType(typeof(Resource<LearningPathDetailDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Resource<LearningPathDetailDto>>> Create(
        [FromBody] LearningPathCreateDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var entity = dto.FromCreateDto();
        _context.LearningPaths.Add(entity);
        await _context.SaveChangesAsync();

        // relaciona vídeos, se vierem IDs
        if (dto.VideoIds is not null)
        {
            int order = 1;
            foreach (var videoId in dto.VideoIds.Distinct())
            {
                _context.LearningPathVideos.Add(new LearningPathVideo
                {
                    LearningPathId = entity.Id,
                    VideoId = videoId,
                    Order = order++
                });
            }
            await _context.SaveChangesAsync();
        }

        var created = await _context.LearningPaths
            .Include(l => l.Profession)
            .Include(l => l.LearningPathVideos)
                .ThenInclude(lpv => lpv.Video)
            .AsNoTracking()
            .FirstAsync(l => l.Id == entity.Id);

        var detailDto = created.ToDetailDto();

        var resource = new Resource<LearningPathDetailDto>
        {
            Data = detailDto,
            Links = BuildLearningPathLinks(created.Id)
        };

        return CreatedAtRoute(
            routeName: "GetLearningPathById",
            routeValues: new { id = created.Id },
            value: resource
        );
    }

    // PUT api/learningpaths/1
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, [FromBody] LearningPathUpdateDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var entity = await _context.LearningPaths
            .Include(l => l.LearningPathVideos)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (entity == null) return NotFound();

        entity.UpdateFromDto(dto);

        // se VideoIds vierem, sobrescreve associação de vídeos
        if (dto.VideoIds is not null)
        {
            _context.LearningPathVideos.RemoveRange(entity.LearningPathVideos);

            int order = 1;
            foreach (var videoId in dto.VideoIds.Distinct())
            {
                _context.LearningPathVideos.Add(new LearningPathVideo
                {
                    LearningPathId = entity.Id,
                    VideoId = videoId,
                    Order = order++
                });
            }
        }

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // DELETE api/learningpaths/1
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _context.LearningPaths
            .Include(l => l.LearningPathVideos)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (entity == null) return NotFound();

        _context.LearningPathVideos.RemoveRange(entity.LearningPathVideos);
        _context.LearningPaths.Remove(entity);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // ===== HATEOAS helpers =====

    private IEnumerable<LinkDto> BuildLearningPathLinks(int id)
    {
        var links = new List<LinkDto>
        {
            new()
            {
                Href = Url.Link("GetLearningPathById", new { id })!,
                Rel = "self",
                Method = "GET"
            },
            new()
            {
                Href = Url.Link("GetLearningPaths", new { pageNumber = 1, pageSize = 10 })!,
                Rel = "learningpaths",
                Method = "GET"
            },
            new()
            {
                Href = Url.Action(nameof(Update), values: new { id })!,
                Rel = "update_learningpath",
                Method = "PUT"
            },
            new()
            {
                Href = Url.Action(nameof(Delete), values: new { id })!,
                Rel = "delete_learningpath",
                Method = "DELETE"
            }
        };

        return links;
    }

    private IEnumerable<LinkDto> BuildLearningPathsCollectionLinks(int pageNumber, int pageSize, int totalPages)
    {
        var links = new List<LinkDto>
        {
            new()
            {
                Href = Url.Link("GetLearningPaths", new { pageNumber, pageSize })!,
                Rel = "self",
                Method = "GET"
            }
        };

        if (pageNumber > 1)
        {
            links.Add(new LinkDto
            {
                Href = Url.Link("GetLearningPaths", new { pageNumber = pageNumber - 1, pageSize })!,
                Rel = "prev_page",
                Method = "GET"
            });
        }

        if (pageNumber < totalPages)
        {
            links.Add(new LinkDto
            {
                Href = Url.Link("GetLearningPaths", new { pageNumber = pageNumber + 1, pageSize })!,
                Rel = "next_page",
                Method = "GET"
            });
        }

        return links;
    }
}
