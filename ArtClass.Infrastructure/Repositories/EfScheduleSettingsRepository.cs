using Microsoft.EntityFrameworkCore;
using ArtClass.Domain.Entities;
using ArtClass.Domain.Repositories;
using ArtClass.Domain.Services;
using ArtClass.Infrastructure.Data;

namespace ArtClass.Infrastructure.Repositories;

internal sealed class EfScheduleSettingsRepository(ArtClassDbContext context)
    : EfRepository<ScheduleSettings>(context), IScheduleSettingsRepository
{
    public async Task<ScheduleSettings> GetOrCreateAsync(CancellationToken cancellationToken = default)
    {
        var settings = await Context.ScheduleSettings.FirstOrDefaultAsync(cancellationToken);
        if (settings is not null)
        {
            return settings;
        }

        settings = new ScheduleSettings
        {
            CycleStartDate = ScheduleCycle.GetMondayOfWeek(DateOnly.FromDateTime(DateTime.Today)),
        };

        await Context.ScheduleSettings.AddAsync(settings, cancellationToken);
        await Context.SaveChangesAsync(cancellationToken);
        return settings;
    }
}
