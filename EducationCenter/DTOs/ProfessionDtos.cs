using System.Collections.Generic;

namespace EducationalCenter.Api.DTOs;

public class ProfessionDto
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
}

public class ProfessionDetailDto
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string MarketOverview { get; set; } = default!;

    public IEnumerable<LearningPathSummaryDto> LearningPaths { get; set; }
        = new List<LearningPathSummaryDto>();
}
