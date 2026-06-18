using Microsoft.EntityFrameworkCore;
using ArtClass.Domain.Entities;
using ArtClass.Domain.Repositories;
using ArtClass.Infrastructure.Data;

namespace ArtClass.Infrastructure.Repositories;

internal sealed class EfLessonRepository(ArtClassDbContext context)
    : EfRepository<Lesson>(context), ILessonRepository
{
    public async Task<IReadOnlyList<Lesson>> GetByDayAsync(
        DayOfWeek day,
        CancellationToken cancellationToken = default) =>
        await Context.Lessons
            .AsNoTracking()
            .Include(l => l.StudyGroup)
            .Include(l => l.Teacher)
            .Include(l => l.Subject)
            .Include(l => l.Classroom)
            .Where(l => l.DayOfWeek == day)
            .OrderBy(l => l.StartTime)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Lesson>> GetWeekScheduleAsync(
        CancellationToken cancellationToken = default) =>
        await Context.Lessons
            .AsNoTracking()
            .Include(l => l.StudyGroup)
            .Include(l => l.Teacher)
            .Include(l => l.Subject)
            .Include(l => l.Classroom)
            .OrderBy(l => l.DayOfWeek)
            .ThenBy(l => l.StartTime)
            .ToListAsync(cancellationToken);
}
