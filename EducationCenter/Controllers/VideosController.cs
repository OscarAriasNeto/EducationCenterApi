using EducationCenter.Data;
using EducationCenter.DTOs;
using EducationCenter.Models;
using Microsoft.AspNetCore.Mvc;
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

    // GET api/videos
    [HttpGet]
    public async Task<ActionResult<IEnumerable<VideoDto>>> GetAll()
    {
        var videos = await _context.Videos.ToListAsync();
        var dtos = videos.Select(v => v.ToDto()).ToList();
        return Ok(dtos);
    }

    // GET api/videos/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<VideoDto>> GetById(int id)
    {
        var video = await _context.Videos.FindAsync(id);
        if (video == null) return NotFound();

        return Ok(video.ToDto());
    }

    // GET api/videos/by-trail/1
    [HttpGet("by-trail/{learningPathId:int}")]
    public async Task<ActionResult<IEnumerable<VideoDto>>> GetByLearningPath(int learningPathId)
    {
        var videos = await _context.LearningPathVideos
            .Where(lp => lp.LearningPathId == learningPathId)
            .Include(lp => lp.Video)
            .OrderBy(lp => lp.Order)
            .Select(lp => lp.Video)
            .ToListAsync();

        var dtos = videos.Select(v => v.ToDto()).ToList();
        return Ok(dtos);
    }

    // POST api/videos
    [HttpPost]
    public async Task<ActionResult<VideoDto>> Create([FromBody] VideoCreateDto dto)
    {
        var entity = dto.FromCreateDto();
        _context.Videos.Add(entity);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { id = entity.Id },
            entity.ToDto()
        );
    }

    // PUT api/videos/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] VideoUpdateDto dto)
    {
        var entity = await _context.Videos.FindAsync(id);
        if (entity == null) return NotFound();

        entity.UpdateFromDto(dto);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE api/videos/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _context.Videos.FindAsync(id);
        if (entity == null) return NotFound();

        _context.Videos.Remove(entity);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
