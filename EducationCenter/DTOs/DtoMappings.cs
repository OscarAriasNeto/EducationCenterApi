using System.Linq;
using EducationalCenter.Api.DTOs;
using EducationalCenter.Api.Models;

namespace EducationalCenter.Api.DTOs;

public static class DtoMappings
{
    // ===== Profession =====
    public static ProfessionDto ToDto(this Profession entity)
        => new()
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description
        };

    public static ProfessionDetailDto ToDetailDto(this Profession entity)
        => new()
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            MarketOverview = entity.MarketOverview,
            LearningPaths = entity.LearningPaths?
                .Select(lp => lp.ToSummaryDto())
                .ToList() ?? new()
        };

    // ===== Video =====
    public static VideoDto ToDto(this Video entity)
        => new()
        {
            Id = entity.Id,
            Title = entity.Title,
            Description = entity.Description,
            Url = entity.Url,
            DurationMinutes = entity.DurationMinutes
        };

    // ===== LearningPath =====
    public static LearningPathSummaryDto ToSummaryDto(this LearningPath entity)
        => new()
        {
            Id = entity.Id,
            Title = entity.Title,
            ProfessionName = entity.Profession?.Name
        };

    public static LearningPathDetailDto ToDetailDto(this LearningPath entity)
        => new()
        {
            Id = entity.Id,
            Title = entity.Title,
            Description = entity.Description,
            ProfessionId = entity.ProfessionId,
            ProfessionName = entity.Profession?.Name ?? string.Empty,
            Videos = entity.LearningPathVideos?
                .OrderBy(lpv => lpv.Order)
                .Select(lpv => lpv.Video.ToDto())
                .ToList() ?? new()
        };

    // ===== Student =====
    public static StudentDto ToDto(this Student entity)
        => new()
        {
            Id = entity.Id,
            FullName = entity.FullName,
            Email = entity.Email,
            BirthDate = entity.BirthDate,
            TargetProfessionName = entity.TargetProfession?.Name
        };

    public static StudentDetailDto ToDetailDto(this Student entity)
        => new()
        {
            Id = entity.Id,
            FullName = entity.FullName,
            Email = entity.Email,
            BirthDate = entity.BirthDate,
            TargetProfessionId = entity.TargetProfessionId,
            TargetProfessionName = entity.TargetProfession?.Name,
            EnrolledLearningPaths = entity.StudentLearningPaths?
                .Select(sl => new StudentLearningPathDto
                {
                    LearningPathId = sl.LearningPathId,
                    LearningPathTitle = sl.LearningPath.Title,
                    ProgressPercent = sl.ProgressPercent,
                    EnrollmentDate = sl.EnrollmentDate
                })
                .ToList() ?? new()
        };
}
