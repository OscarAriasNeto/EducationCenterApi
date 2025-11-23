using EducationCenter.Data;
using EducationCenter.DTOs;
using EducationCenter.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EducationCenter.Controllers.v2;

[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/learningpaths")]
public class LearningPathsController : ControllerBase
{
    private readonly EducationalCenterContext _context;
    private readonly ILogger<LearningPathsController> _logger;

    public LearningPathsController(EducationalCenterContext context, ILogger<LearningPathsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet(Name = "GetLearningPathsV2")]
    public async Task<ActionResult<PagedResponse<LearningPathSummaryDto>>> GetAll(
        string? title = null,
        int? professionId = null,
        int pageNumber = 1,
        int pageSize = 10)
    {
        var query = _context.LearningPaths
            .Include(lp => lp.Profession)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(title))
        {
            var upper = title.ToUpperInvariant();
            query = query.Where(lp => lp.Title.ToUpper().Contains(upper));
        }

        if (professionId.HasValue)
        {
            query = query.Where(lp => lp.ProfessionId == professionId.Value);
        }

        var total = await query.CountAsync();
        var pages = (int)Math.Ceiling(total / (double)pageSize);

        var data = await query
            .OrderBy(x => x.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new PagedResponse<LearningPathSummaryDto>
        {
            Items = data.Select(lp => lp.ToSummaryDto()).ToList(),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = total,
            TotalPages = pages
        });
    }

    [HttpGet("{id:int}", Name = "GetLearningPathByIdV2")]
    public async Task<ActionResult<Resource<LearningPathDetailDto>>> GetById(int id)
    {
        var lp = await _context.LearningPaths
            .Include(lp => lp.Profession)
            .Include(lp => lp.LearningPathVideos)
                .ThenInclude(x => x.Video)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (lp == null) return NotFound();

        return Ok(new Resource<LearningPathDetailDto>
        {
            Data = lp.ToDetailDto()
        });
    }

    [HttpPost]
    public async Task<ActionResult<Resource<LearningPathDetailDto>>> Create(LearningPathCreateDto dto)
    {
        var entity = dto.FromCreateDto();
        _context.LearningPaths.Add(entity);
        await _context.SaveChangesAsync();

        if (dto.VideoIds != null)
        {
            int order = 1;
            foreach (var vid in dto.VideoIds)
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
                .ThenInclude(x => x.Video)
            .AsNoTracking()
            .FirstAsync(lp => lp.Id == entity.Id);

        return CreatedAtRoute(
            "GetLearningPathByIdV2",
            new { id = entity.Id, version = "2.0" },
            new Resource<LearningPathDetailDto> { Data = created.ToDetailDto() }
        );
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, LearningPathUpdateDto dto)
    {
        var entity = await _context.LearningPaths
            .Include(lp => lp.LearningPathVideos)
            .FirstOrDefaultAsync(lp => lp.Id == id);

        if (entity == null) return NotFound();

        entity.UpdateFromDto(dto);

        if (dto.VideoIds != null)
        {
            _context.LearningPathVideos.RemoveRange(entity.LearningPathVideos);

            int order = 1;
            foreach (var vid in dto.VideoIds)
            {
                _context.LearningPathVideos.Add(new LearningPathVideo
                {
                    LearningPathId = entity.Id,
                    VideoId = vid,
                    Order = order++
                });
            }
        }

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _context.LearningPaths
            .Include(lp => lp.LearningPathVideos)
            .FirstOrDefaultAsync(lp => lp.Id == id);

        if (entity == null) return NotFound();

        _context.LearningPathVideos.RemoveRange(entity.LearningPathVideos);
        _context.LearningPaths.Remove(entity);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
