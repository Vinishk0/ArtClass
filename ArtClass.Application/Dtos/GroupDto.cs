namespace ArtClass.Application.Dtos;

public sealed record GroupDto(
    int Id,
    string Name,
    string? Description,
    string Color,
    bool IsRepeating,
    bool IsBiWeekly,
    IReadOnlyList<GroupSlotDto> Slots,
    IReadOnlyList<int> StudentIds,
    int StudentCount);

public sealed record GroupSlotDto(
    int Id,
    DayOfWeek DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime,
    int? CycleWeek,
    DateOnly? SpecificDate);
