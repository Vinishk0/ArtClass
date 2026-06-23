namespace ArtClass.Application.Dtos;

public sealed record ScheduleCycleDto(
    DateOnly CycleStartDate,
    DateOnly CycleEndDate,
    int CurrentWeek);
