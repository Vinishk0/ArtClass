namespace ArtClass.Domain.Services;

public sealed record LessonCalendarSlot(
    int? CycleWeek,
    DayOfWeek DayOfWeek,
    DateOnly? SpecificDate,
    string GroupColor);
