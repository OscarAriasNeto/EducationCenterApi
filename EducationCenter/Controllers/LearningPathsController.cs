using EducationalCenter.Api.Data;
using EducationalCenter.Api.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EducationalCenter.Api.Controllers;

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
}
