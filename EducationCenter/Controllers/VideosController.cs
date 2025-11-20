using EducationalCenter.Api.Data;
using EducationalCenter.Api.Models;
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
    public async Task<ActionResult<IEnumerable<Video>>> GetAll()
    {
        return await _context.Videos.ToListAsync();
    }

    // GET api/videos/by-trail/1
    [HttpGet("by-trail/{learningPathId:int}")]
    public async Task<ActionResult<IEnumerable<Video>>> GetByLearningPath(int learningPathId)
    {
        var videos = await _context.LearningPathVideos
            .Where(lp => lp.LearningPathId == learningPathId)
            .OrderBy(lp => lp.Order)
            .Select(lp => lp.Video)
            .ToListAsync();

        return videos;
    }
}
