using ArtClass.Domain.Entities;

namespace ArtClass.Domain.Repositories;

public interface ILessonRepository : IRepository<Lesson>
{
    Task<IReadOnlyList<Lesson>> GetByDayAsync(DayOfWeek day, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Lesson>> GetWeekScheduleAsync(CancellationToken cancellationToken = default);
}
