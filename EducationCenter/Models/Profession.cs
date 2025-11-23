using System.Collections.Generic;

namespace EducationCenter.Models;

public class Profession
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string MarketOverview { get; set; } = string.Empty;

    public ICollection<LearningPath> LearningPaths { get; set; } = new List<LearningPath>();
    public ICollection<Student> Students { get; set; } = new List<Student>();
}
