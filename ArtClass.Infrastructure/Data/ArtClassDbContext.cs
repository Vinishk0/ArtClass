using Microsoft.EntityFrameworkCore;
using ArtClass.Domain.Entities;

namespace ArtClass.Infrastructure.Data;

public class ArtClassDbContext(DbContextOptions<ArtClassDbContext> options) : DbContext(options)
{
    public DbSet<Teacher> Teachers => Set<Teacher>();

    public DbSet<StudyGroup> StudyGroups => Set<StudyGroup>();

    public DbSet<Subject> Subjects => Set<Subject>();

    public DbSet<Classroom> Classrooms => Set<Classroom>();

    public DbSet<Lesson> Lessons => Set<Lesson>();

    public DbSet<Student> Students => Set<Student>();

    public DbSet<StudentStudyGroup> StudentStudyGroups => Set<StudentStudyGroup>();

    public DbSet<ScheduleSettings> ScheduleSettings => Set<ScheduleSettings>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ArtClassDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
