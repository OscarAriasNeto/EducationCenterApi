using System.Collections.Generic;

namespace EducationCenter.Models;

public class Video
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }

    public ICollection<LearningPathVideo> LearningPathVideos { get; set; } = new List<LearningPathVideo>();
}
