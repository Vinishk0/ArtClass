using ArtClass.Application.Dtos;

namespace ArtClass.Application.Services;

public interface IScheduleService
{
    Task<IReadOnlyList<LessonDto>> GetWeekScheduleAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LessonDto>> GetDayScheduleAsync(DayOfWeek day, CancellationToken cancellationToken = default);
}
