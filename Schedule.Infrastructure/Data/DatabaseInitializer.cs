using Microsoft.EntityFrameworkCore;
using Schedule.Domain.Entities;
using Schedule.Infrastructure.Data;

namespace Schedule.Infrastructure.Data;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(ScheduleDbContext context, CancellationToken cancellationToken = default)
    {
        await context.Database.EnsureCreatedAsync(cancellationToken);

        if (await context.Lessons.AnyAsync(cancellationToken))
        {
            return;
        }

        var teachers = new[]
        {
            new Teacher { FullName = "Иванова Мария Петровна", Specialization = "Живопись" },
            new Teacher { FullName = "Смирнов Алексей Викторович", Specialization = "Рисунок" },
            new Teacher { FullName = "Козлова Елена Сергеевна", Specialization = "Скульптура" },
        };

        var groups = new[]
        {
            new StudyGroup { Name = "Детская группа А", Description = "7–10 лет" },
            new StudyGroup { Name = "Подростковая группа Б", Description = "11–14 лет" },
            new StudyGroup { Name = "Взрослая студия", Description = "18+" },
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

        context.Teachers.AddRange(teachers);
        context.StudyGroups.AddRange(groups);
        context.Subjects.AddRange(subjects);
        context.Classrooms.AddRange(classrooms);
        await context.SaveChangesAsync(cancellationToken);

        context.Lessons.AddRange(
            new Lesson
            {
                StudyGroupId = groups[0].Id,
                TeacherId = teachers[0].Id,
                SubjectId = subjects[0].Id,
                ClassroomId = classrooms[0].Id,
                DayOfWeek = DayOfWeek.Monday,
                StartTime = new TimeOnly(10, 0),
                EndTime = new TimeOnly(11, 30),
            },
            new Lesson
            {
                StudyGroupId = groups[1].Id,
                TeacherId = teachers[1].Id,
                SubjectId = subjects[1].Id,
                ClassroomId = classrooms[1].Id,
                DayOfWeek = DayOfWeek.Monday,
                StartTime = new TimeOnly(14, 0),
                EndTime = new TimeOnly(15, 30),
            },
            new Lesson
            {
                StudyGroupId = groups[2].Id,
                TeacherId = teachers[2].Id,
                SubjectId = subjects[2].Id,
                ClassroomId = classrooms[2].Id,
                DayOfWeek = DayOfWeek.Wednesday,
                StartTime = new TimeOnly(18, 0),
                EndTime = new TimeOnly(20, 0),
            },
            new Lesson
            {
                StudyGroupId = groups[0].Id,
                TeacherId = teachers[0].Id,
                SubjectId = subjects[3].Id,
                ClassroomId = classrooms[0].Id,
                DayOfWeek = DayOfWeek.Friday,
                StartTime = new TimeOnly(10, 0),
                EndTime = new TimeOnly(11, 30),
            });

        await context.SaveChangesAsync(cancellationToken);
    }
}
