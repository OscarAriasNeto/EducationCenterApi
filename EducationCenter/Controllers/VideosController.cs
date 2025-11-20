using EducationCenter.Data;
using EducationCenter.DTOs;
using EducationCenter.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace EducationCenter.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VideosController : ControllerBase
{
    private readonly EducationalCenterContext _context;

    public VideosController(EducationalCenterContext context)
    {
        _context = context;
    }

    // GET api/videos?pageNumber=1&pageSize=10
    [HttpGet(Name = "GetVideos")]
    [ProducesResponseType(typeof(PagedResponse<VideoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<VideoDto>>> GetAll(
        int pageNumber = 1,
        int pageSize = 10)
    {
        if (pageNumber <= 0 || pageSize <= 0)
            return BadRequest("pageNumber e pageSize devem ser maiores que zero.");

        var query = _context.Videos.AsNoTracking();

        var totalItems = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        var entities = await query
            .OrderBy(v => v.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = entities.Select(v => v.ToDto()).ToList();

        var response = new PagedResponse<VideoDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalPages,
            Links = BuildVideosCollectionLinks(pageNumber, pageSize, totalPages)
        };

        return Ok(response);
    }

    // GET api/videos/5
    [HttpGet("{id:int}", Name = "GetVideoById")]
    [ProducesResponseType(typeof(Resource<VideoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Resource<VideoDto>>> GetById(int id)
    {
        var video = await _context.Videos
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == id);

        if (video == null) return NotFound();

        var dto = video.ToDto();

        var resource = new Resource<VideoDto>
        {
            Data = dto,
            Links = BuildVideoLinks(id)
        };

        return Ok(resource);
    }

    // GET api/videos/by-trail/1
    [HttpGet("by-trail/{learningPathId:int}")]
    [ProducesResponseType(typeof(IEnumerable<VideoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<VideoDto>>> GetByLearningPath(int learningPathId)
    {
        var videos = await _context.LearningPathVideos
            .Where(lp => lp.LearningPathId == learningPathId)
            .Include(lp => lp.Video)
            .OrderBy(lp => lp.Order)
            .Select(lp => lp.Video)
            .AsNoTracking()
            .ToListAsync();

        var dtos = videos.Select(v => v.ToDto()).ToList();
        return Ok(dtos);
    }

    // POST api/videos
    [HttpPost]
    [ProducesResponseType(typeof(Resource<VideoDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Resource<VideoDto>>> Create([FromBody] VideoCreateDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var entity = dto.FromCreateDto();
        _context.Videos.Add(entity);
        await _context.SaveChangesAsync();

        var created = await _context.Videos
            .AsNoTracking()
            .FirstAsync(v => v.Id == entity.Id);

        var createdDto = created.ToDto();

        var resource = new Resource<VideoDto>
        {
            Data = createdDto,
            Links = BuildVideoLinks(created.Id)
        };

        return CreatedAtRoute(
            routeName: "GetVideoById",
            routeValues: new { id = created.Id },
            value: resource
        );
    }

    // PUT api/videos/5
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, [FromBody] VideoUpdateDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var entity = await _context.Videos.FindAsync(id);
        if (entity == null) return NotFound();

        entity.UpdateFromDto(dto);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE api/videos/5
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _context.Videos.FindAsync(id);
        if (entity == null) return NotFound();

        _context.Videos.Remove(entity);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // ===== HATEOAS helpers =====

    private IEnumerable<LinkDto> BuildVideoLinks(int id)
    {
        var links = new List<LinkDto>
        {
            new()
            {
                Href = Url.Link("GetVideoById", new { id })!,
                Rel = "self",
                Method = "GET"
            },
            new()
            {
                Href = Url.Link("GetVideos", new { pageNumber = 1, pageSize = 10 })!,
                Rel = "videos",
                Method = "GET"
            },
            new()
            {
                Href = Url.Action(nameof(Update), values: new { id })!,
                Rel = "update_video",
                Method = "PUT"
            },
            new()
            {
                Href = Url.Action(nameof(Delete), values: new { id })!,
                Rel = "delete_video",
                Method = "DELETE"
            }
        };

        return links;
    }

    private IEnumerable<LinkDto> BuildVideosCollectionLinks(int pageNumber, int pageSize, int totalPages)
    {
        var links = new List<LinkDto>
        {
            new()
            {
                Href = Url.Link("GetVideos", new { pageNumber, pageSize })!,
                Rel = "self",
                Method = "GET"
            }
        };

        if (pageNumber > 1)
        {
            links.Add(new LinkDto
            {
                Href = Url.Link("GetVideos", new { pageNumber = pageNumber - 1, pageSize })!,
                Rel = "prev_page",
                Method = "GET"
            });
        }

        if (pageNumber < totalPages)
        {
            links.Add(new LinkDto
            {
                Href = Url.Link("GetVideos", new { pageNumber = pageNumber + 1, pageSize })!,
                Rel = "next_page",
                Method = "GET"
            });
        }

        return links;
    }
}
