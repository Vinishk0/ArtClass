namespace ArtClass.Application.Services;

public interface IScheduleRollService
{
    /// <summary>
    /// Advances the 2-week cycle anchor by 14 days and removes expired one-time lessons.
    /// Intended to run weekly (e.g. every Sunday).
    /// </summary>
    Task RollAsync(CancellationToken cancellationToken = default);
}
