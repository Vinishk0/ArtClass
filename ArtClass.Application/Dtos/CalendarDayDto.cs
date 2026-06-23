namespace ArtClass.Application.Dtos;

public sealed record CalendarDayDto(
    DateOnly Date,
    bool IsCurrentMonth,
    bool IsToday,
    int LessonCount,
    IReadOnlyList<string> GroupColors);
