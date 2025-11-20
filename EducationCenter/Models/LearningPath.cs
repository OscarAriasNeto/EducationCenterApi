using EducationCenter.Models;

namespace EducationalCenter.Api.Models;

public class LearningPath
{
    public int Id { get; set; }
    public string Title { get; set; } = default!;         // Ex: "Trilha Back-end Júnior"
    public string Description { get; set; } = default!;

    public int ProfessionId { get; set; }
    public Profession Profession { get; set; } = default!;

    public ICollection<LearningPathVideo> LearningPathVideos { get; set; } = new List<LearningPathVideo>();
    public ICollection<StudentLearningPath> StudentLearningPaths { get; set; } = new List<StudentLearningPath>();
}
