namespace EducationCenter.Models;

public class StudentLearningPath
{
    public int StudentId { get; set; }
    public Student Student { get; set; } = default!;

    public int LearningPathId { get; set; }
    public LearningPath LearningPath { get; set; } = default!;

    public DateTime EnrollmentDate { get; set; }
    public int ProgressPercent { get; set; }
}
