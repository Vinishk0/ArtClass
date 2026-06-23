using ArtClass.Application.Dtos;

namespace ArtClass.Application.Services;

public interface IScheduleService
{
    Task<IReadOnlyList<LessonDto>> GetWeekScheduleAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LessonDto>> GetDayScheduleAsync(DayOfWeek day, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LessonDto>> GetDayScheduleByDateAsync(DateOnly date, CancellationToken cancellationToken = default);

    Task<ScheduleCycleDto> GetCurrentCycleAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CalendarDayDto>> GetCalendarDaysAsync(int year, int month, CancellationToken cancellationToken = default);
}
