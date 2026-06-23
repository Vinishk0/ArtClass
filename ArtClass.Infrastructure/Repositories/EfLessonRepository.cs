using Microsoft.EntityFrameworkCore;
using ArtClass.Domain.Services;
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
            .Where(l => l.StudyGroup.IsRepeating)
            .OrderBy(l => l.CycleWeek)
            .ThenBy(l => l.DayOfWeek)
            .ThenBy(l => l.StartTime)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Lesson>> GetByDateAsync(
        DateOnly date,
        DateOnly cycleStartDate,
        CancellationToken cancellationToken = default)
    {
        var cycleWeek = ScheduleCycle.GetCycleWeek(date, cycleStartDate);
        var dayOfWeek = date.DayOfWeek;

        return await Context.Lessons
            .AsNoTracking()
            .Include(l => l.StudyGroup)
            .Include(l => l.Teacher)
            .Include(l => l.Subject)
            .Include(l => l.Classroom)
            .Where(l =>
                (l.StudyGroup.IsRepeating
                 && l.DayOfWeek == dayOfWeek
                 && (l.CycleWeek == null || l.CycleWeek == cycleWeek))
                || (l.SpecificDate == date))
            .OrderBy(l => l.StartTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Lesson>> GetRepeatingTemplatesAsync(
        CancellationToken cancellationToken = default) =>
        await Context.Lessons
            .AsNoTracking()
            .Include(l => l.StudyGroup)
            .Where(l => l.StudyGroup.IsRepeating && l.SpecificDate == null)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<LessonCalendarSlot>> GetRepeatingCalendarSlotsAsync(
        CancellationToken cancellationToken = default) =>
        await Context.Lessons
            .AsNoTracking()
            .Where(l => l.StudyGroup.IsRepeating && l.SpecificDate == null)
            .Select(l => new LessonCalendarSlot(
                l.CycleWeek,
                l.DayOfWeek,
                null,
                l.StudyGroup.Color))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Lesson>> GetOneTimeInRangeAsync(
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default) =>
        await Context.Lessons
            .AsNoTracking()
            .Include(l => l.StudyGroup)
            .Where(l => l.SpecificDate != null && l.SpecificDate >= from && l.SpecificDate <= to)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<LessonCalendarSlot>> GetOneTimeCalendarSlotsInRangeAsync(
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default) =>
        await Context.Lessons
            .AsNoTracking()
            .Where(l => l.SpecificDate != null && l.SpecificDate >= from && l.SpecificDate <= to)
            .Select(l => new LessonCalendarSlot(
                null,
                l.DayOfWeek,
                l.SpecificDate,
                l.StudyGroup.Color))
            .ToListAsync(cancellationToken);

    public async Task DeleteByStudyGroupIdAsync(
        int studyGroupId,
        CancellationToken cancellationToken = default)
    {
        var lessons = await Context.Lessons
            .Where(l => l.StudyGroupId == studyGroupId)
            .ToListAsync(cancellationToken);

        if (lessons.Count > 0)
        {
            Context.Lessons.RemoveRange(lessons);
        }
    }

    public async Task DeleteExpiredOneTimeLessonsAsync(
        DateOnly beforeDate,
        CancellationToken cancellationToken = default)
    {
        var expired = await Context.Lessons
            .Where(l => l.SpecificDate != null && l.SpecificDate < beforeDate)
            .ToListAsync(cancellationToken);

        if (expired.Count == 0)
        {
            return;
        }

        Context.Lessons.RemoveRange(expired);
    }
}
