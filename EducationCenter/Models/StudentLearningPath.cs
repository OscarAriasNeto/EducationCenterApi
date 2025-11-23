using System;

namespace EducationCenter.Models;

public class StudentLearningPath
{
    public int StudentId { get; set; }
    public int LearningPathId { get; set; }
    public DateTime EnrollmentDate { get; set; }
    public int ProgressPercent { get; set; }

    public Student Student { get; set; } = null!;
    public LearningPath LearningPath { get; set; } = null!;
}
