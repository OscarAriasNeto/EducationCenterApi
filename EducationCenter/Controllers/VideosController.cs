using EducationalCenter.Api.Data;
using EducationalCenter.Api.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EducationalCenter.Api.Controllers;

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
}
