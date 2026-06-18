using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Schedule.Domain.Entities;

namespace Schedule.Infrastructure.Data.Configurations;

internal sealed class TeacherConfiguration : IEntityTypeConfiguration<Teacher>
{
    public void Configure(EntityTypeBuilder<Teacher> builder)
    {
        builder.Property(t => t.FullName).HasMaxLength(200).IsRequired();
        builder.Property(t => t.Specialization).HasMaxLength(200);
    }
}

internal sealed class StudyGroupConfiguration : IEntityTypeConfiguration<StudyGroup>
{
    public void Configure(EntityTypeBuilder<StudyGroup> builder)
    {
        builder.Property(g => g.Name).HasMaxLength(100).IsRequired();
        builder.Property(g => g.Description).HasMaxLength(500);
    }
}

internal sealed class SubjectConfiguration : IEntityTypeConfiguration<Subject>
{
    public void Configure(EntityTypeBuilder<Subject> builder)
    {
        builder.Property(s => s.Name).HasMaxLength(100).IsRequired();
    }
}

internal sealed class ClassroomConfiguration : IEntityTypeConfiguration<Classroom>
{
    public void Configure(EntityTypeBuilder<Classroom> builder)
    {
        builder.Property(c => c.Name).HasMaxLength(50).IsRequired();
    }
}

internal sealed class LessonConfiguration : IEntityTypeConfiguration<Lesson>
{
    public void Configure(EntityTypeBuilder<Lesson> builder)
    {
        builder.Property(l => l.Notes).HasMaxLength(500);

        builder.HasOne(l => l.StudyGroup)
            .WithMany(g => g.Lessons)
            .HasForeignKey(l => l.StudyGroupId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.Teacher)
            .WithMany(t => t.Lessons)
            .HasForeignKey(l => l.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.Subject)
            .WithMany(s => s.Lessons)
            .HasForeignKey(l => l.SubjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.Classroom)
            .WithMany(c => c.Lessons)
            .HasForeignKey(l => l.ClassroomId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(l => new { l.DayOfWeek, l.StartTime });
    }
}
