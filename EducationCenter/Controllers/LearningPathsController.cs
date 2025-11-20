using EducationCenter.Data;
using EducationCenter.DTOs;
using EducationCenter.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EducationCenter.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LearningPathsController : ControllerBase
{
    private readonly EducationalCenterContext _context;
    private readonly ILogger<LearningPathsController> _logger;

    public LearningPathsController(
        EducationalCenterContext context,
        ILogger<LearningPathsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET api/learningpaths?pageNumber=1&pageSize=10
    [HttpGet(Name = "GetLearningPaths")]
    public async Task<ActionResult<PagedResponse<LearningPathSummaryDto>>> GetAll(
        int pageNumber = 1,
        int pageSize = 10)
    {
        _logger.LogInformation("GET /learningpaths page={Page} size={Size}", pageNumber, pageSize);

        var query = _context.LearningPaths
            .Include(lp => lp.Profession)
            .AsNoTracking();

        var totalItems = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        var items = await query
            .OrderBy(lp => lp.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(lp => lp.ToSummaryDto())
            .ToListAsync();

        return Ok(new PagedResponse<LearningPathSummaryDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalPages,
            Links = BuildPathsCollectionLinks(pageNumber, pageSize, totalPages)
        });
    }

    // GET api/learningpaths/1
    [HttpGet("{id:int}", Name = "GetLearningPathById")]
    public async Task<ActionResult<Resource<LearningPathDetailDto>>> GetById(int id)
    {
        _logger.LogInformation("GET /learningpaths/{Id}", id);

        var lp = await _context.LearningPaths
            .Include(l => l.Profession)
            .Include(l => l.LearningPathVideos)
            .ThenInclude(lpv => lpv.Video)
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == id);

        if (lp == null)
            return NotFound();

        var dto = lp.ToDetailDto();

        return Ok(new Resource<LearningPathDetailDto>
        {
            Data = dto,
            Links = BuildPathLinks(id)
        });
    }

    // POST api/learningpaths
    [HttpPost]
    public async Task<ActionResult<Resource<LearningPathDetailDto>>> Create([FromBody] LearningPathCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var entity = dto.FromCreateDto();
        _context.LearningPaths.Add(entity);
        await _context.SaveChangesAsync();

        // associa vídeos
        if (dto.VideoIds is not null)
        {
            int order = 1;
            foreach (var vid in dto.VideoIds.Distinct())
            {
                _context.LearningPathVideos.Add(new LearningPathVideo
                {
                    LearningPathId = entity.Id,
                    VideoId = vid,
                    Order = order++
                });
            }
            await _context.SaveChangesAsync();
        }

        var created = await _context.LearningPaths
            .Include(lp => lp.Profession)
            .Include(lp => lp.LearningPathVideos)
            .ThenInclude(lpv => lpv.Video)
            .AsNoTracking()
            .FirstAsync(lp => lp.Id == entity.Id);

        return CreatedAtRoute("GetLearningPathById", new { id = entity.Id }, new Resource<LearningPathDetailDto>
        {
            Data = created.ToDetailDto(),
            Links = BuildPathLinks(entity.Id)
        });
    }

    // PUT api/learningpaths/1
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] LearningPathUpdateDto dto)
    {
        var entity = await _context.LearningPaths
            .Include(lp => lp.LearningPathVideos)
            .FirstOrDefaultAsync(lp => lp.Id == id);

        if (entity == null)
            return NotFound();

        entity.UpdateFromDto(dto);

        if (dto.VideoIds is not null)
        {
            _context.LearningPathVideos.RemoveRange(entity.LearningPathVideos);

            int order = 1;
            foreach (var vid in dto.VideoIds.Distinct())
            {
                _context.LearningPathVideos.Add(new LearningPathVideo
                {
                    LearningPathId = id,
                    VideoId = vid,
                    Order = order++
                });
            }
        }

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // DELETE api/learningpaths/1
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _context.LearningPaths
            .Include(lp => lp.LearningPathVideos)
            .FirstOrDefaultAsync(lp => lp.Id == id);

        if (entity == null)
            return NotFound();

        _context.LearningPathVideos.RemoveRange(entity.LearningPathVideos);
        _context.LearningPaths.Remove(entity);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // ===== HATEOAS =====
    private IEnumerable<LinkDto> BuildPathLinks(int id)
    {
        return new List<LinkDto>
        {
            new() { Href = Url.Link("GetLearningPathById", new { id })!, Rel = "self", Method = "GET" },
            new() { Href = Url.Link("GetLearningPaths", new { pageNumber = 1, pageSize = 10 })!, Rel = "all", Method = "GET" },
            new() { Href = Url.Action(nameof(Update), new { id })!, Rel = "update", Method = "PUT" },
            new() { Href = Url.Action(nameof(Delete), new { id })!, Rel = "delete", Method = "DELETE" }
        };
    }

    private IEnumerable<LinkDto> BuildPathsCollectionLinks(int pageNumber, int pageSize, int totalPages)
    {
        var links = new List<LinkDto>
        {
            new() { Href = Url.Link("GetLearningPaths", new { pageNumber, pageSize })!, Rel = "self", Method = "GET" }
        };

        if (pageNumber > 1)
            links.Add(new LinkDto { Href = Url.Link("GetLearningPaths", new { pageNumber = pageNumber - 1, pageSize })!, Rel = "prev", Method = "GET" });

        if (pageNumber < totalPages)
            links.Add(new LinkDto { Href = Url.Link("GetLearningPaths", new { pageNumber = pageNumber + 1, pageSize })!, Rel = "next", Method = "GET" });

        return links;
    }
}
