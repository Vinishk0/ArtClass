using ArtClass.Domain.Common;

namespace ArtClass.Domain.Entities;

/// <summary>
/// Singleton row (Id = 1) — anchor Monday of the current 2-week cycle.
/// </summary>
public class ScheduleSettings : Entity
{
    public DateOnly CycleStartDate { get; set; }
}
