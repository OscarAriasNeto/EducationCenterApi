using EducationCenter.Data;
using EducationCenter.DTOs;
using EducationCenter.Models;
using Microsoft.AspNetCore.Mvc;
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

    // GET api/learningpaths
    [HttpGet]
    public async Task<ActionResult<IEnumerable<LearningPathSummaryDto>>> GetAll()
    {
        var entities = await _context.LearningPaths
            .Include(lp => lp.Profession)
            .ToListAsync();

        var dtos = entities.Select(lp => lp.ToSummaryDto()).ToList();
        return Ok(dtos);
    }

    // GET api/learningpaths/1
    [HttpGet("{id:int}")]
    public async Task<ActionResult<LearningPathDetailDto>> GetById(int id)
    {
        var lp = await _context.LearningPaths
            .Include(l => l.Profession)
            .Include(l => l.LearningPathVideos)
                .ThenInclude(x => x.Video)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (lp == null) return NotFound();

        return Ok(lp.ToDetailDto());
    }

    // POST api/learningpaths
    [HttpPost]
    public async Task<ActionResult<LearningPathDetailDto>> Create([FromBody] LearningPathCreateDto dto)
    {
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
            .FirstAsync(l => l.Id == entity.Id);

        return CreatedAtAction(
            nameof(GetById),
            new { id = created.Id },
            created.ToDetailDto()
        );
    }

    // PUT api/learningpaths/1
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] LearningPathUpdateDto dto)
    {
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
}
