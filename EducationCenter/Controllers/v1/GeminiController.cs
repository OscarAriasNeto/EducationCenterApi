using EducationCenter.Services;
using Microsoft.AspNetCore.Mvc;

namespace EducationCenter.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/gemini")]
public class GeminiController : ControllerBase
{
    private readonly GeminiClient _geminiClient;

    public GeminiController(GeminiClient geminiClient)
    {
        _geminiClient = geminiClient;
    }

    [HttpPost("ask")]
    public async Task<ActionResult<object>> Ask([FromBody] GeminiRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Prompt))
            return BadRequest("Prompt é obrigatório.");

        var answer = await _geminiClient.AskAsync(request.Prompt);

        return Ok(new
        {
            request.Prompt,
            Answer = answer
        });
    }
}

public class GeminiRequest
{
    public string Prompt { get; set; } = string.Empty;
}
