using System;
using System.Collections.Generic;

namespace EducationalCenter.Api.DTOs;

public class StudentDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public DateTime BirthDate { get; set; }
    public string? TargetProfessionName { get; set; }
}

public class StudentLearningPathDto
{
    public int LearningPathId { get; set; }
    public string LearningPathTitle { get; set; } = default!;
    public int ProgressPercent { get; set; }
    public DateTime EnrollmentDate { get; set; }
}

public class StudentDetailDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public DateTime BirthDate { get; set; }

    public int? TargetProfessionId { get; set; }
    public string? TargetProfessionName { get; set; }

    public IEnumerable<StudentLearningPathDto> EnrolledLearningPaths { get; set; }
        = new List<StudentLearningPathDto>();
}
