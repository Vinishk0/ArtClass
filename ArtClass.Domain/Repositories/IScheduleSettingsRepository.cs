using ArtClass.Domain.Entities;

namespace ArtClass.Domain.Repositories;

public interface IScheduleSettingsRepository : IRepository<ScheduleSettings>
{
    Task<ScheduleSettings> GetOrCreateAsync(CancellationToken cancellationToken = default);
}
