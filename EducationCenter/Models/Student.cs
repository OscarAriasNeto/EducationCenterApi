using System;
using System.Collections.Generic;

namespace EducationCenter.Models;

public class Student
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }

    public int? TargetProfessionId { get; set; }
    public Profession? TargetProfession { get; set; }

    public ICollection<StudentLearningPath> StudentLearningPaths { get; set; } = new List<StudentLearningPath>();
}
