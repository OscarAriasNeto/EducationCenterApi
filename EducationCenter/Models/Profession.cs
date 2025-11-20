using EducationCenter.Models;

namespace EducationalCenter.Api.Models;

public class Profession
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;          // Ex: "Desenvolvedor Back-end"
    public string Description { get; set; } = default!;
    public string MarketOverview { get; set; } = default!; // Breve texto sobre o mercado

    public ICollection<LearningPath> LearningPaths { get; set; } = new List<LearningPath>();
}
