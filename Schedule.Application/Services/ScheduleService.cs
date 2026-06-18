using Schedule.Application.Dtos;
using Schedule.Domain.Entities;
using Schedule.Domain.Repositories;

namespace Schedule.Application.Services;

public sealed class ScheduleService(IUnitOfWork unitOfWork) : IScheduleService
{
    public async Task<IReadOnlyList<LessonDto>> GetWeekScheduleAsync(CancellationToken cancellationToken = default)
    {
        var lessons = await unitOfWork.Lessons.GetWeekScheduleAsync(cancellationToken);
        return MapLessons(lessons);
    }

    public async Task<IReadOnlyList<LessonDto>> GetDayScheduleAsync(
        DayOfWeek day,
        CancellationToken cancellationToken = default)
    {
        var lessons = await unitOfWork.Lessons.GetByDayAsync(day, cancellationToken);
        return MapLessons(lessons);
    }

    private static IReadOnlyList<LessonDto> MapLessons(IReadOnlyList<Lesson> lessons) =>
        lessons
            .Select(lesson => new LessonDto(
                lesson.Id,
                lesson.StudyGroup.Name,
                lesson.Teacher.FullName,
                lesson.Subject.Name,
                lesson.Classroom.Name,
                lesson.DayOfWeek,
                lesson.StartTime,
                lesson.EndTime,
                lesson.Notes))
            .ToList();
}
