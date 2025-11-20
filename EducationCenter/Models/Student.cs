using EducationCenter.Models;

namespace EducationalCenter.Api.Models;

public class Student
{
    public int Id { get; set; }
    public string FullName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public DateTime BirthDate { get; set; }

    // Profissão de interesse do aluno
    public int? TargetProfessionId { get; set; }
    public Profession? TargetProfession { get; set; }

    public ICollection<StudentLearningPath> StudentLearningPaths { get; set; } = new List<StudentLearningPath>();
}
