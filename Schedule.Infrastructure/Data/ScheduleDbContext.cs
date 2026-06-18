using Microsoft.EntityFrameworkCore;
using Schedule.Domain.Entities;

namespace Schedule.Infrastructure.Data;

public class ScheduleDbContext(DbContextOptions<ScheduleDbContext> options) : DbContext(options)
{
    public DbSet<Teacher> Teachers => Set<Teacher>();

    public DbSet<StudyGroup> StudyGroups => Set<StudyGroup>();

    public DbSet<Subject> Subjects => Set<Subject>();

    public DbSet<Classroom> Classrooms => Set<Classroom>();

    public DbSet<Lesson> Lessons => Set<Lesson>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ScheduleDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
