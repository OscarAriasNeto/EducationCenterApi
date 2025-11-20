using EducationalCenter.Api.Models;

namespace EducationCenter.Models;

public class Student
{
    public int Id { get; set; }
    public string FullName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public DateTime BirthDate { get; set; }

    public int? TargetProfessionId { get; set; }
    public Profession? TargetProfession { get; set; }

    public ICollection<StudentLearningPath> StudentLearningPaths { get; set; } = new List<StudentLearningPath>();
}
