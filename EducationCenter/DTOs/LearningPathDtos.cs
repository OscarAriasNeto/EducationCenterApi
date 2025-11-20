using System.Collections.Generic;

namespace EducationCenter.DTOs;

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

public class LearningPathCreateDto
{
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public int ProfessionId { get; set; }
    public IEnumerable<int>? VideoIds { get; set; }
}

public class LearningPathUpdateDto
{
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public int ProfessionId { get; set; }
    public IEnumerable<int>? VideoIds { get; set; }
}
