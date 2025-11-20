namespace EducationalCenter.Api.DTOs;

public class VideoDto
{
    public int Id { get; set; }
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string Url { get; set; } = default!;
    public int DurationMinutes { get; set; }
}
