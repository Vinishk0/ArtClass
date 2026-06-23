namespace ArtClass.Application.Dtos;

public sealed record CreateGroupRequest(
    string Name,
    string? Description,
    string Color,
    bool IsBiWeekly,
    IReadOnlyList<GroupSlotInput> Slots,
    IReadOnlyList<int> StudentIds);

public sealed record GroupSlotInput(
    DayOfWeek DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime,
    int? CycleWeek);

public sealed record CreateExtraLessonRequest(
    string Name,
    DateOnly Date,
    TimeOnly StartTime,
    TimeOnly EndTime,
    string? Notes,
    string Color,
    IReadOnlyList<int> StudentIds);

public sealed record UpdateExtraLessonRequest(
    string Name,
    DateOnly Date,
    TimeOnly StartTime,
    TimeOnly EndTime,
    string? Notes,
    string Color,
    IReadOnlyList<int> StudentIds);
