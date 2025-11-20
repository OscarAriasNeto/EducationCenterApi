using EducationalCenter.Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace EducationalCenter.Api.Data;

public class EducationalCenterContext : DbContext
{
    public EducationalCenterContext(DbContextOptions<EducationalCenterContext> options)
    : base(options)
    {
    }

    public DbSet<Profession> Professions => Set<Profession>();
    public DbSet<LearningPath> LearningPaths => Set<LearningPath>();
    public DbSet<Video> Videos => Set<Video>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<LearningPathVideo> LearningPathVideos => Set<LearningPathVideo>();
    public DbSet<StudentLearningPath> StudentLearningPaths => Set<StudentLearningPath>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<LearningPathVideo>()
            .HasKey(lp => new { lp.LearningPathId, lp.VideoId });

        modelBuilder.Entity<StudentLearningPath>()
            .HasKey(sl => new { sl.StudentId, sl.LearningPathId });

        modelBuilder.Entity<Profession>().HasData(
            new Profession
            {
                Id = 1,
                Name = "Desenvolvedor Back-end",
                Description = "Responsável pela regra de negócio e acesso a dados.",
                MarketOverview = "Alta demanda em empresas de tecnologia e bancos."
            },
            new Profession
            {
                Id = 2,
                Name = "Desenvolvedor Front-end",
                Description = "Focado em interfaces e experiência do usuário.",
                MarketOverview = "Muito procurado para produtos digitais e startups."
            },
            new Profession
            {
                Id = 3,
                Name = "Analista de Dados",
                Description = "Transforma dados em insights para o negócio.",
                MarketOverview = "Área em expansão em diversos segmentos."
            }
        );
    }
}
