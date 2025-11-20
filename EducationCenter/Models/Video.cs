using EducationCenter.Models;

namespace EducationalCenter.Api.Models;

public class Video
{
    public int Id { get; set; }
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string Url { get; set; } = default!;       // Link do vídeo (YouTube, etc)
    public int DurationMinutes { get; set; }

    public ICollection<LearningPathVideo> LearningPathVideos { get; set; } = new List<LearningPathVideo>();
}
