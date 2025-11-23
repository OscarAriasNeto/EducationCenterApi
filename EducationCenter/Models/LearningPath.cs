using System.Collections.Generic;

namespace EducationCenter.Models;

public class LearningPath
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public int ProfessionId { get; set; }
    public Profession Profession { get; set; } = null!;

    public ICollection<LearningPathVideo> LearningPathVideos { get; set; } = new List<LearningPathVideo>();
    public ICollection<StudentLearningPath> StudentLearningPaths { get; set; } = new List<StudentLearningPath>();
}
