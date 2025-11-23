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

    public class GeminiRequest
    {
        public string Prompt { get; set; } = string.Empty;
    }

    public class GeminiResponse
    {
        public string Prompt { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
    }

    [HttpPost("ask")]
    [ProducesResponseType(typeof(GeminiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<GeminiResponse>> Ask([FromBody] GeminiRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Prompt))
            return BadRequest("Prompt é obrigatório.");

        var answer = await _geminiClient.AskAsync(request.Prompt);

        var response = new GeminiResponse
        {
            Prompt = request.Prompt,
            Answer = answer
        };

        return Ok(response);
    }
}
