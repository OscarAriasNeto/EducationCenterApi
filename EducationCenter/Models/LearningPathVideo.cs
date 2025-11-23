namespace EducationCenter.Models;

public class LearningPathVideo
{
    public int LearningPathId { get; set; }
    public int VideoId { get; set; }
    public int Order { get; set; }

    public LearningPath LearningPath { get; set; } = null!;
    public Video Video { get; set; } = null!;
}
