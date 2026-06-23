using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ArtClass.Domain.Entities;

namespace ArtClass.Infrastructure.Data.Configurations;

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
        builder.Property(g => g.IsRepeating).HasDefaultValue(true);
        builder.Property(g => g.IsBiWeekly).HasDefaultValue(true);
        builder.Property(g => g.Color).HasMaxLength(9).IsRequired().HasDefaultValue("#C45C3E");
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

        builder.HasIndex(l => l.SpecificDate);
    }
}

internal sealed class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.Property(s => s.FullName).HasMaxLength(200).IsRequired();
        builder.Property(s => s.Phone).HasMaxLength(30);
    }
}

internal sealed class StudentStudyGroupConfiguration : IEntityTypeConfiguration<StudentStudyGroup>
{
    public void Configure(EntityTypeBuilder<StudentStudyGroup> builder)
    {
        builder.HasKey(e => new { e.StudentId, e.StudyGroupId });

        builder.HasOne(e => e.Student)
            .WithMany(s => s.GroupEnrollments)
            .HasForeignKey(e => e.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.StudyGroup)
            .WithMany(g => g.StudentEnrollments)
            .HasForeignKey(e => e.StudyGroupId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class ScheduleSettingsConfiguration : IEntityTypeConfiguration<ScheduleSettings>
{
    public void Configure(EntityTypeBuilder<ScheduleSettings> builder)
    {
        builder.Property(s => s.CycleStartDate).IsRequired();
    }
}
