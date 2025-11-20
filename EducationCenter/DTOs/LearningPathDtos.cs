using System.Collections.Generic;

namespace EducationalCenter.Api.DTOs;

public class LearningPathSummaryDto
{
    public int Id { get; set; }
    public string Title { get; set; } = default!;
    public string? ProfessionName { get; set; }
}

public class LearningPathDetailDto
{
    public int Id { get; set; }
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;

    public int ProfessionId { get; set; }
    public string ProfessionName { get; set; } = default!;

    public IEnumerable<VideoDto> Videos { get; set; } = new List<VideoDto>();
}
