using EducationCenter.Data;
using EducationCenter.DTOs;
using EducationCenter.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EducationCenter.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VideosController : ControllerBase
{
    private readonly EducationalCenterContext _context;
    private readonly ILogger<VideosController> _logger;

    public VideosController(
        EducationalCenterContext context,
        ILogger<VideosController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET api/videos?pageNumber=1&pageSize=10
    [HttpGet(Name = "GetVideos")]
    [ProducesResponseType(typeof(PagedResponse<VideoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<VideoDto>>> GetAll(
        int pageNumber = 1,
        int pageSize = 10)
    {
        _logger.LogInformation("GET /videos page={Page} size={Size}", pageNumber, pageSize);

        if (pageNumber <= 0 || pageSize <= 0)
            return BadRequest("Os parâmetros de paginação devem ser maiores que zero.");

        var query = _context.Videos.AsNoTracking();

        var totalItems = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        var items = await query
            .OrderBy(v => v.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(v => v.ToDto())
            .ToListAsync();

        return Ok(new PagedResponse<VideoDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalPages,
            Links = BuildVideosCollectionLinks(pageNumber, pageSize, totalPages)
        });
    }

    // GET api/videos/5
    [HttpGet("{id:int}", Name = "GetVideoById")]
    [ProducesResponseType(typeof(Resource<VideoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Resource<VideoDto>>> GetById(int id)
    {
        _logger.LogInformation("GET /videos/{Id}", id);

        var video = await _context.Videos.AsNoTracking().FirstOrDefaultAsync(v => v.Id == id);
        if (video == null)
            return NotFound();

        return Ok(new Resource<VideoDto>
        {
            Data = video.ToDto(),
            Links = BuildVideoLinks(id)
        });
    }

    // GET api/videos/by-trail/1
    [HttpGet("by-trail/{learningPathId:int}")]
    public async Task<ActionResult<IEnumerable<VideoDto>>> GetByLearningPath(int learningPathId)
    {
        var videos = await _context.LearningPathVideos
            .Where(lp => lp.LearningPathId == learningPathId)
            .Include(lp => lp.Video)
            .OrderBy(lp => lp.Order)
            .Select(lp => lp.Video.ToDto())
            .ToListAsync();

        return Ok(videos);
    }

    // POST api/videos
    [HttpPost]
    [ProducesResponseType(typeof(Resource<VideoDto>), StatusCodes.Status201Created)]
    public async Task<ActionResult<Resource<VideoDto>>> Create([FromBody] VideoCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var entity = dto.FromCreateDto();
        _context.Videos.Add(entity);
        await _context.SaveChangesAsync();

        var resource = new Resource<VideoDto>
        {
            Data = entity.ToDto(),
            Links = BuildVideoLinks(entity.Id)
        };

        return CreatedAtRoute("GetVideoById", new { id = entity.Id }, resource);
    }

    // PUT api/videos/5
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Update(int id, [FromBody] VideoUpdateDto dto)
    {
        var entity = await _context.Videos.FindAsync(id);
        if (entity == null)
            return NotFound();

        entity.UpdateFromDto(dto);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE api/videos/5
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _context.Videos.FindAsync(id);
        if (entity == null)
            return NotFound();

        _context.Videos.Remove(entity);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // ===== HATEOAS Helpers =====
    private IEnumerable<LinkDto> BuildVideoLinks(int id)
    {
        return new List<LinkDto>
        {
            new() { Href = Url.Link("GetVideoById", new { id })!, Rel = "self", Method = "GET" },
            new() { Href = Url.Link("GetVideos", new { pageNumber = 1, pageSize = 10 })!, Rel = "all", Method = "GET" },
            new() { Href = Url.Action(nameof(Update), new { id })!, Rel = "update", Method = "PUT" },
            new() { Href = Url.Action(nameof(Delete), new { id })!, Rel = "delete", Method = "DELETE" }
        };
    }

    private IEnumerable<LinkDto> BuildVideosCollectionLinks(int pageNumber, int pageSize, int totalPages)
    {
        var links = new List<LinkDto>
        {
            new() { Href = Url.Link("GetVideos", new { pageNumber, pageSize })!, Rel = "self", Method = "GET" }
        };

        if (pageNumber > 1)
            links.Add(new LinkDto { Href = Url.Link("GetVideos", new { pageNumber = pageNumber - 1, pageSize })!, Rel = "prev", Method = "GET" });

        if (pageNumber < totalPages)
            links.Add(new LinkDto { Href = Url.Link("GetVideos", new { pageNumber = pageNumber + 1, pageSize })!, Rel = "next", Method = "GET" });

        return links;
    }
}
