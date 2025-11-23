using EducationCenter.Models;
using Microsoft.EntityFrameworkCore;

namespace EducationCenter.Data;

public class EducationalCenterContext : DbContext
{
    public EducationalCenterContext(DbContextOptions<EducationalCenterContext> options)
        : base(options)
    {
    }

    public DbSet<Student> Students => Set<Student>();
    public DbSet<Profession> Professions => Set<Profession>();
    public DbSet<LearningPath> LearningPaths => Set<LearningPath>();
    public DbSet<LearningPathVideo> LearningPathVideos => Set<LearningPathVideo>();
    public DbSet<StudentLearningPath> StudentLearningPaths => Set<StudentLearningPath>();
    public DbSet<Video> Videos => Set<Video>();
    public DbSet<Audit> Audits => Set<Audit>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ==========================
        // PROFESSION -> EC_PROFESSION
        // ==========================
        modelBuilder.Entity<Profession>(entity =>
        {
            entity.ToTable("EC_PROFESSION");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                  .HasColumnName("PROFESSION_ID");

            entity.Property(e => e.Name)
                  .HasColumnName("NAME")
                  .HasMaxLength(100)
                  .IsRequired();

            entity.Property(e => e.Description)
                  .HasColumnName("DESCRIPTION")
                  .HasMaxLength(255)
                  .IsRequired();

            entity.Property(e => e.MarketOverview)
                  .HasColumnName("MARKET_OVERVIEW")
                  .IsRequired();
        });

        // ==========================
        // VIDEO -> EC_VIDEO
        // ==========================
        modelBuilder.Entity<Video>(entity =>
        {
            entity.ToTable("EC_VIDEO");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                  .HasColumnName("VIDEO_ID");

            entity.Property(e => e.Title)
                  .HasColumnName("TITLE")
                  .HasMaxLength(150)
                  .IsRequired();

            entity.Property(e => e.Description)
                  .HasColumnName("DESCRIPTION")
                  .HasMaxLength(255)
                  .IsRequired();

            entity.Property(e => e.Url)
                  .HasColumnName("URL")
                  .HasMaxLength(255)
                  .IsRequired();

            entity.Property(e => e.DurationMinutes)
                  .HasColumnName("DURATION_MINUTES")
                  .IsRequired();
        });

        // ==========================
        // LEARNING PATH -> EC_LEARNING_PATH
        // ==========================
        modelBuilder.Entity<LearningPath>(entity =>
        {
            entity.ToTable("EC_LEARNING_PATH");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                  .HasColumnName("LEARNING_PATH_ID");

            entity.Property(e => e.Title)
                  .HasColumnName("TITLE")
                  .HasMaxLength(150)
                  .IsRequired();

            entity.Property(e => e.Description)
                  .HasColumnName("DESCRIPTION")
                  .HasMaxLength(255)
                  .IsRequired();

            entity.Property(e => e.ProfessionId)
                  .HasColumnName("PROFESSION_ID")
                  .IsRequired();

            entity.HasOne(e => e.Profession)
                  .WithMany(p => p.LearningPaths)
                  .HasForeignKey(e => e.ProfessionId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ==========================
        // STUDENT -> EC_STUDENT
        // ==========================
        modelBuilder.Entity<Student>(entity =>
        {
            entity.ToTable("EC_STUDENT");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                  .HasColumnName("STUDENT_ID");

            entity.Property(e => e.FullName)
                  .HasColumnName("FULL_NAME")
                  .HasMaxLength(150)
                  .IsRequired();

            entity.Property(e => e.Email)
                  .HasColumnName("EMAIL")
                  .HasMaxLength(150)
                  .IsRequired();

            entity.HasIndex(e => e.Email)
                  .IsUnique()
                  .HasDatabaseName("UK_STUDENT__EMAIL");

            entity.Property(e => e.BirthDate)
                  .HasColumnName("BIRTH_DATE")
                  .IsRequired();

            entity.Property(e => e.TargetProfessionId)
                  .HasColumnName("TARGET_PROFESSION_ID");

            // relação com Profession (alvo do aluno)
            entity.HasOne(e => e.TargetProfession)
                  .WithMany(p => p.Students)
                  .HasForeignKey(e => e.TargetProfessionId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ==========================
        // LEARNING_PATH_VIDEO -> EC_LEARNING_PATH_VIDEO
        // ==========================
        modelBuilder.Entity<LearningPathVideo>(entity =>
        {
            entity.ToTable("EC_LEARNING_PATH_VIDEO");

            entity.HasKey(e => new { e.LearningPathId, e.VideoId });

            entity.Property(e => e.LearningPathId)
                  .HasColumnName("LEARNING_PATH_ID");

            entity.Property(e => e.VideoId)
                  .HasColumnName("VIDEO_ID");

            // coluna "ORDER" (reservada, vem com aspas no SQL)
            entity.Property(e => e.Order)
                  .HasColumnName("ORDER")   // o provider Oracle faz o escape necessário
                  .IsRequired();

            entity.HasOne(e => e.LearningPath)
                  .WithMany(lp => lp.LearningPathVideos)
                  .HasForeignKey(e => e.LearningPathId);

            entity.HasOne(e => e.Video)
                  .WithMany(v => v.LearningPathVideos)
                  .HasForeignKey(e => e.VideoId);
        });

        // ==========================
        // STUDENT_LEARNING_PATH -> EC_STUDENT_LEARNING_PATH
        // ==========================
        modelBuilder.Entity<StudentLearningPath>(entity =>
        {
            entity.ToTable("EC_STUDENT_LEARNING_PATH");

            entity.HasKey(e => new { e.StudentId, e.LearningPathId });

            entity.Property(e => e.StudentId)
                  .HasColumnName("STUDENT_ID");

            entity.Property(e => e.LearningPathId)
                  .HasColumnName("LEARNING_PATH_ID");

            entity.Property(e => e.EnrollmentDate)
                  .HasColumnName("ENROLLMENT_DATE")
                  .IsRequired();

            entity.Property(e => e.ProgressPercent)
                  .HasColumnName("PROGRESS_PERCENT")
                  .HasDefaultValue(0);

            entity.HasOne(e => e.Student)
                  .WithMany(s => s.StudentLearningPaths)
                  .HasForeignKey(e => e.StudentId);

            entity.HasOne(e => e.LearningPath)
                  .WithMany(lp => lp.StudentLearningPaths)
                  .HasForeignKey(e => e.LearningPathId);
        });

        // ==========================
        // AUDIT -> EC_AUDIT (opcional)
        // ==========================
        modelBuilder.Entity<Audit>(entity =>
        {
            entity.ToTable("EC_AUDIT");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                  .HasColumnName("AUDIT_ID");

            entity.Property(e => e.TableName)
                  .HasColumnName("TABLE_NAME");

            entity.Property(e => e.Operation)
                  .HasColumnName("OPERATION");

            entity.Property(e => e.PkValue)
                  .HasColumnName("PK_VALUE");

            entity.Property(e => e.BeforeJson)
                  .HasColumnName("BEFORE_JSON");

            entity.Property(e => e.AfterJson)
                  .HasColumnName("AFTER_JSON");

            entity.Property(e => e.DoneAt)
                  .HasColumnName("DONE_AT");

            entity.Property(e => e.DoneBy)
                  .HasColumnName("DONE_BY");
        });
    }
}
