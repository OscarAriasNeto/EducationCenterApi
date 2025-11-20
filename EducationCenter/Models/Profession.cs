namespace EducationCenter.Models;

public class Profession
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string MarketOverview { get; set; } = default!;

    public ICollection<LearningPath> LearningPaths { get; set; } = new List<LearningPath>();
}
