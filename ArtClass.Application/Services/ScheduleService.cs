using ArtClass.Application.Caching;
using ArtClass.Application.Data;
using ArtClass.Application.Dtos;
using ArtClass.Domain.Entities;
using ArtClass.Domain.Services;

namespace ArtClass.Application.Services;

public sealed class ScheduleService(UnitOfWorkExecutor db, IAppDataCache cache) : IScheduleService
{
    private const string RepeatingSlotsCacheKey = "calendar:repeating-slots";

    private sealed record RepeatingSlotIndex(
        IReadOnlyDictionary<DayOfWeek, IReadOnlyList<LessonCalendarSlot>> Weekly,
        IReadOnlyDictionary<(int CycleWeek, DayOfWeek DayOfWeek), IReadOnlyList<LessonCalendarSlot>> BiWeekly);

    public async Task<IReadOnlyList<LessonDto>> GetWeekScheduleAsync(CancellationToken cancellationToken = default)
    {
        var lessons = await db.QueryAsync(
            (unitOfWork, ct) => unitOfWork.Lessons.GetWeekScheduleAsync(ct),
            cancellationToken);
        return MapLessons(lessons);
    }

    public async Task<IReadOnlyList<LessonDto>> GetDayScheduleAsync(
        DayOfWeek day,
        CancellationToken cancellationToken = default)
    {
        var lessons = await db.QueryAsync(
            (unitOfWork, ct) => unitOfWork.Lessons.GetByDayAsync(day, ct),
            cancellationToken);
        return MapLessons(lessons);
    }

    public async Task<IReadOnlyList<LessonDto>> GetDayScheduleByDateAsync(
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        var key = $"day:{date:yyyy-MM-dd}";
        if (cache.TryGet(key, out IReadOnlyList<LessonDto>? cached) && cached is not null)
        {
            return cached;
        }

        var settings = await db.QueryAsync(
            (unitOfWork, ct) => unitOfWork.ScheduleSettings.GetOrCreateAsync(ct),
            cancellationToken);
        var lessons = await db.QueryAsync(
            (unitOfWork, ct) => unitOfWork.Lessons.GetByDateAsync(date, settings.CycleStartDate, ct),
            cancellationToken);
        var mapped = MapLessons(lessons);
        cache.Set(key, mapped);
        return mapped;
    }

    public async Task<ScheduleCycleDto> GetCurrentCycleAsync(CancellationToken cancellationToken = default)
    {
        const string key = "cycle:current";
        if (cache.TryGet(key, out ScheduleCycleDto? cached) && cached is not null)
        {
            return cached;
        }

        var settings = await db.QueryAsync(
            (unitOfWork, ct) => unitOfWork.ScheduleSettings.GetOrCreateAsync(ct),
            cancellationToken);
        var today = DateOnly.FromDateTime(DateTime.Today);
        var currentWeek = ScheduleCycle.GetCycleWeek(today, settings.CycleStartDate);
        var cycleEnd = settings.CycleStartDate.AddDays(ScheduleCycle.WeeksInCycle * 7 - 1);
        var dto = new ScheduleCycleDto(settings.CycleStartDate, cycleEnd, currentWeek);
        cache.Set(key, dto);
        return dto;
    }

    public async Task<IReadOnlyList<CalendarDayDto>> GetCalendarDaysAsync(
        int year,
        int month,
        CancellationToken cancellationToken = default)
    {
        var key = $"calendar:{year}:{month}";
        if (cache.TryGet(key, out IReadOnlyList<CalendarDayDto>? cached) && cached is not null)
        {
            return cached;
        }

        var settings = await db.QueryAsync(
            (unitOfWork, ct) => unitOfWork.ScheduleSettings.GetOrCreateAsync(ct),
            cancellationToken);
        var today = DateOnly.FromDateTime(DateTime.Today);
        var firstOfMonth = new DateOnly(year, month, 1);
        var startOffset = ((int)firstOfMonth.DayOfWeek + 6) % 7;
        var gridStart = firstOfMonth.AddDays(-startOffset);
        var gridEnd = gridStart.AddDays(41);

        var repeatingSlots = await GetRepeatingSlotsAsync(cancellationToken);
        var oneTimeByDate = await GetOneTimeSlotsByDateAsync(gridStart, gridEnd, cancellationToken);

        var days = new List<CalendarDayDto>(42);
        for (var i = 0; i < 42; i++)
        {
            var date = gridStart.AddDays(i);

            var repeating = GetRepeatingSlotsForDate(date, settings.CycleStartDate, repeatingSlots);
            oneTimeByDate.TryGetValue(date, out var extra);

            extra ??= [];

            var lessonCount = repeating.Count + extra.Count;
            var colors = new HashSet<string>(repeating.Count + extra.Count);
            foreach (var slot in repeating)
            {
                colors.Add(GroupColors.Normalize(slot.GroupColor));
            }

            foreach (var slot in extra)
            {
                colors.Add(GroupColors.Normalize(slot.GroupColor));
            }

            days.Add(new CalendarDayDto(
                date,
                date.Month == month,
                date == today,
                lessonCount,
                colors.ToList()));
        }

        cache.Set(key, days);
        return days;
    }

    private async Task<RepeatingSlotIndex> GetRepeatingSlotsAsync(CancellationToken cancellationToken)
    {
        if (cache.TryGet(RepeatingSlotsCacheKey, out RepeatingSlotIndex? cached) && cached is not null)
        {
            return cached;
        }

        var slots = await db.QueryAsync(
            (unitOfWork, ct) => unitOfWork.Lessons.GetRepeatingCalendarSlotsAsync(ct),
            cancellationToken);

        var weekly = slots
            .Where(s => s.CycleWeek is null)
            .GroupBy(s => s.DayOfWeek)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<LessonCalendarSlot>)g.ToList());

        var biWeekly = slots
            .Where(s => s.CycleWeek is not null)
            .GroupBy(s => (s.CycleWeek!.Value, s.DayOfWeek))
            .ToDictionary(g => g.Key, g => (IReadOnlyList<LessonCalendarSlot>)g.ToList());

        var index = new RepeatingSlotIndex(weekly, biWeekly);
        cache.Set(RepeatingSlotsCacheKey, index);
        return index;
    }

    private static IReadOnlyList<LessonCalendarSlot> GetRepeatingSlotsForDate(
        DateOnly date,
        DateOnly cycleStartDate,
        RepeatingSlotIndex index)
    {
        var cycleWeek = ScheduleCycle.GetCycleWeek(date, cycleStartDate);
        var dayOfWeek = date.DayOfWeek;

        index.Weekly.TryGetValue(dayOfWeek, out var weekly);
        index.BiWeekly.TryGetValue((cycleWeek, dayOfWeek), out var biWeekly);

        if (weekly is null && biWeekly is null)
        {
            return [];
        }

        if (weekly is null)
        {
            return biWeekly!;
        }

        if (biWeekly is null)
        {
            return weekly;
        }

        var combined = new List<LessonCalendarSlot>(weekly.Count + biWeekly.Count);
        combined.AddRange(weekly);
        combined.AddRange(biWeekly);
        return combined;
    }

    private async Task<IReadOnlyDictionary<DateOnly, IReadOnlyList<LessonCalendarSlot>>> GetOneTimeSlotsByDateAsync(
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken)
    {
        var key = $"calendar:onetime:{from:yyyy-MM-dd}:{to:yyyy-MM-dd}";
        if (cache.TryGet(key, out IReadOnlyDictionary<DateOnly, IReadOnlyList<LessonCalendarSlot>>? cached)
            && cached is not null)
        {
            return cached;
        }

        var slots = await db.QueryAsync(
            (unitOfWork, ct) => unitOfWork.Lessons.GetOneTimeCalendarSlotsInRangeAsync(from, to, ct),
            cancellationToken);
        var grouped = slots
            .Where(s => s.SpecificDate is not null)
            .GroupBy(s => s.SpecificDate!.Value)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<LessonCalendarSlot>)g.ToList());

        cache.Set(key, grouped);
        return grouped;
    }

    private static IReadOnlyList<LessonDto> MapLessons(IReadOnlyList<Lesson> lessons) =>
        lessons
            .Select(lesson => new LessonDto(
                lesson.Id,
                lesson.StudyGroupId,
                lesson.StudyGroup.Name,
                GroupColors.Normalize(lesson.StudyGroup.Color),
                lesson.Teacher.FullName,
                lesson.Subject.Name,
                lesson.Classroom.Name,
                lesson.DayOfWeek,
                lesson.StartTime,
                lesson.EndTime,
                lesson.Notes,
                lesson.StudyGroup.IsRepeating,
                lesson.CycleWeek,
                lesson.SpecificDate))
            .ToList();
}
