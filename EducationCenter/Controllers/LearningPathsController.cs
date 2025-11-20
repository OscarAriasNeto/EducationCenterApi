using EducationalCenter.Api.Data;
using EducationalCenter.Api.Models;
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
    public async Task<ActionResult<IEnumerable<LearningPath>>> GetAll()
    {
        return await _context.LearningPaths
            .Include(lp => lp.Profession)
            .ToListAsync();
    }

    // GET api/learningpaths/1
    [HttpGet("{id:int}")]
    public async Task<ActionResult<LearningPath>> GetById(int id)
    {
        var lp = await _context.LearningPaths
            .Include(l => l.Profession)
            .Include(l => l.LearningPathVideos).ThenInclude(x => x.Video)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (lp == null) return NotFound();

        return lp;
    }
}
