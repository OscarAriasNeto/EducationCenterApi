using EducationalCenter.Api.Models;

namespace EducationCenter.Models;

public class LearningPathVideo
{
    public int LearningPathId { get; set; }
    public LearningPath LearningPath { get; set; } = default!;

    public int VideoId { get; set; }
    public Video Video { get; set; } = default!;

    public int Order { get; set; }
}
