using System.Linq;
using EducationCenter.Models;

namespace EducationCenter.DTOs;

public static class DtoMappings
{
    // Profession
    public static ProfessionDto ToDto(this Profession entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        Description = entity.Description
    };

    public static ProfessionDetailDto ToDetailDto(this Profession entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        Description = entity.Description,
        MarketOverview = entity.MarketOverview,
        LearningPaths = entity.LearningPaths?.Select(lp => lp.ToSummaryDto()).ToList() ?? new()
    };

    public static Profession FromCreateDto(this ProfessionCreateDto dto) => new()
    {
        Name = dto.Name,
        Description = dto.Description,
        MarketOverview = dto.MarketOverview
    };

    public static void UpdateFromDto(this Profession entity, ProfessionUpdateDto dto)
    {
        entity.Name = dto.Name;
        entity.Description = dto.Description;
        entity.MarketOverview = dto.MarketOverview;
    }

    // Video
    public static VideoDto ToDto(this Video entity) => new()
    {
        Id = entity.Id,
        Title = entity.Title,
        Description = entity.Description,
        Url = entity.Url,
        DurationMinutes = entity.DurationMinutes
    };

    public static Video FromCreateDto(this VideoCreateDto dto) => new()
    {
        Title = dto.Title,
        Description = dto.Description,
        Url = dto.Url,
        DurationMinutes = dto.DurationMinutes
    };

    public static void UpdateFromDto(this Video entity, VideoUpdateDto dto)
    {
        entity.Title = dto.Title;
        entity.Description = dto.Description;
        entity.Url = dto.Url;
        entity.DurationMinutes = dto.DurationMinutes;
    }

    // LearningPath
    public static LearningPathSummaryDto ToSummaryDto(this LearningPath entity) => new()
    {
        Id = entity.Id,
        Title = entity.Title,
        ProfessionName = entity.Profession?.Name
    };

    public static LearningPathDetailDto ToDetailDto(this LearningPath entity) => new()
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

    public static LearningPath FromCreateDto(this LearningPathCreateDto dto) => new()
    {
        Title = dto.Title,
        Description = dto.Description,
        ProfessionId = dto.ProfessionId
    };

    public static void UpdateFromDto(this LearningPath entity, LearningPathUpdateDto dto)
    {
        entity.Title = dto.Title;
        entity.Description = dto.Description;
        entity.ProfessionId = dto.ProfessionId;
    }

    // Student
    public static StudentDto ToDto(this Student entity) => new()
    {
        Id = entity.Id,
        FullName = entity.FullName,
        Email = entity.Email,
        BirthDate = entity.BirthDate,
        TargetProfessionName = entity.TargetProfession?.Name
    };

    public static StudentDetailDto ToDetailDto(this Student entity) => new()
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

    public static Student FromCreateDto(this StudentCreateDto dto) => new()
    {
        FullName = dto.FullName,
        Email = dto.Email,
        BirthDate = dto.BirthDate,
        TargetProfessionId = dto.TargetProfessionId
    };

    public static void UpdateFromDto(this Student entity, StudentUpdateDto dto)
    {
        entity.FullName = dto.FullName;
        entity.Email = dto.Email;
        entity.BirthDate = dto.BirthDate;
        entity.TargetProfessionId = dto.TargetProfessionId;
    }
}
