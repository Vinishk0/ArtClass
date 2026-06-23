using Microsoft.EntityFrameworkCore;
using ArtClass.Domain.Entities;
using ArtClass.Domain.Services;

namespace ArtClass.Infrastructure.Data;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(ArtClassDbContext context, CancellationToken cancellationToken = default)
    {
        await context.Database.MigrateAsync(cancellationToken);
        await SchemaCompatibility.EnsureAsync(context, cancellationToken);

        if (await context.Lessons.AnyAsync(cancellationToken))
        {
            return;
        }

        await SeedAsync(context, cancellationToken);
    }

    private static async Task SeedAsync(ArtClassDbContext context, CancellationToken cancellationToken)
    {
        var cycleStart = ScheduleCycle.GetMondayOfWeek(DateOnly.FromDateTime(DateTime.Today));

        var teachers = new[]
        {
            new Teacher { FullName = "Иванова Мария Петровна", Specialization = "Живопись" },
            new Teacher { FullName = "Смирнов Алексей Викторович", Specialization = "Рисунок" },
            new Teacher { FullName = "Козлова Елена Сергеевна", Specialization = "Скульптура" },
        };

        var groups = new[]
        {
            new StudyGroup { Name = "Акварель", Description = "7–10 лет", IsRepeating = true },
            new StudyGroup { Name = "Живопись", Description = "11–14 лет", IsRepeating = true },
            new StudyGroup { Name = "Скетчинг", Description = "12+", IsRepeating = true },
            new StudyGroup { Name = "Мастер-класс", Description = "Разовое занятие", IsRepeating = false },
        };

        var subjects = new[]
        {
            new Subject { Name = "Живопись" },
            new Subject { Name = "Рисунок" },
            new Subject { Name = "Скульптура" },
            new Subject { Name = "Композиция" },
        };

        var classrooms = new[]
        {
            new Classroom { Name = "Каб. 101", Capacity = 12 },
            new Classroom { Name = "Каб. 205", Capacity = 15 },
            new Classroom { Name = "Мастерская", Capacity = 8 },
        };

        var students = new[]
        {
            new Student { FullName = "Аня Петрова", Age = 10, Phone = "+7 900 111-22-33" },
            new Student { FullName = "Иван Сидоров", Age = 12, Phone = "+7 900 444-55-66" },
            new Student { FullName = "Мария Л.", Age = 9, Phone = "+7 900 777-88-99" },
        };

        context.Teachers.AddRange(teachers);
        context.StudyGroups.AddRange(groups);
        context.Subjects.AddRange(subjects);
        context.Classrooms.AddRange(classrooms);
        context.Students.AddRange(students);
        context.ScheduleSettings.Add(new ScheduleSettings { CycleStartDate = cycleStart });
        await context.SaveChangesAsync(cancellationToken);

        context.StudentStudyGroups.AddRange(
            new StudentStudyGroup { StudentId = students[0].Id, StudyGroupId = groups[0].Id },
            new StudentStudyGroup { StudentId = students[0].Id, StudyGroupId = groups[2].Id },
            new StudentStudyGroup { StudentId = students[1].Id, StudyGroupId = groups[1].Id },
            new StudentStudyGroup { StudentId = students[2].Id, StudyGroupId = groups[0].Id });

        var extraDate = cycleStart.AddDays(5);

        context.Lessons.AddRange(
            new Lesson
            {
                StudyGroupId = groups[0].Id,
                TeacherId = teachers[0].Id,
                SubjectId = subjects[0].Id,
                ClassroomId = classrooms[0].Id,
                DayOfWeek = DayOfWeek.Monday,
                CycleWeek = 1,
                StartTime = new TimeOnly(10, 0),
                EndTime = new TimeOnly(11, 30),
            },
            new Lesson
            {
                StudyGroupId = groups[0].Id,
                TeacherId = teachers[0].Id,
                SubjectId = subjects[0].Id,
                ClassroomId = classrooms[0].Id,
                DayOfWeek = DayOfWeek.Wednesday,
                CycleWeek = 1,
                StartTime = new TimeOnly(14, 0),
                EndTime = new TimeOnly(15, 30),
            },
            new Lesson
            {
                StudyGroupId = groups[1].Id,
                TeacherId = teachers[1].Id,
                SubjectId = subjects[1].Id,
                ClassroomId = classrooms[1].Id,
                DayOfWeek = DayOfWeek.Tuesday,
                CycleWeek = 1,
                StartTime = new TimeOnly(11, 0),
                EndTime = new TimeOnly(12, 30),
            },
            new Lesson
            {
                StudyGroupId = groups[1].Id,
                TeacherId = teachers[1].Id,
                SubjectId = subjects[1].Id,
                ClassroomId = classrooms[1].Id,
                DayOfWeek = DayOfWeek.Saturday,
                CycleWeek = 2,
                StartTime = new TimeOnly(10, 0),
                EndTime = new TimeOnly(11, 30),
            },
            new Lesson
            {
                StudyGroupId = groups[2].Id,
                TeacherId = teachers[0].Id,
                SubjectId = subjects[1].Id,
                ClassroomId = classrooms[0].Id,
                DayOfWeek = DayOfWeek.Friday,
                CycleWeek = 2,
                StartTime = new TimeOnly(16, 0),
                EndTime = new TimeOnly(17, 30),
            },
            new Lesson
            {
                StudyGroupId = groups[3].Id,
                TeacherId = teachers[2].Id,
                SubjectId = subjects[3].Id,
                ClassroomId = classrooms[2].Id,
                DayOfWeek = extraDate.DayOfWeek,
                SpecificDate = extraDate,
                StartTime = new TimeOnly(15, 0),
                EndTime = new TimeOnly(17, 0),
                Notes = "Разовый мастер-класс",
            });

        await context.SaveChangesAsync(cancellationToken);
    }
}
