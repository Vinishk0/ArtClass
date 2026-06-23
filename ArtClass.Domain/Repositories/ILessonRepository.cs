using ArtClass.Domain.Entities;
using ArtClass.Domain.Services;

namespace ArtClass.Domain.Repositories;

public interface ILessonRepository : IRepository<Lesson>
{
    Task<IReadOnlyList<Lesson>> GetByDayAsync(DayOfWeek day, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Lesson>> GetWeekScheduleAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Lesson>> GetByDateAsync(DateOnly date, DateOnly cycleStartDate, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Lesson>> GetRepeatingTemplatesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LessonCalendarSlot>> GetRepeatingCalendarSlotsAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Lesson>> GetOneTimeInRangeAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LessonCalendarSlot>> GetOneTimeCalendarSlotsInRangeAsync(
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default);

    Task DeleteByStudyGroupIdAsync(int studyGroupId, CancellationToken cancellationToken = default);

    Task DeleteExpiredOneTimeLessonsAsync(DateOnly beforeDate, CancellationToken cancellationToken = default);
}
