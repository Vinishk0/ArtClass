using ArtClass.Application.Caching;
using ArtClass.Application.Data;
using ArtClass.Domain.Services;

namespace ArtClass.Application.Services;

public sealed class ScheduleRollService(UnitOfWorkExecutor db, IAppDataCache cache) : IScheduleRollService
{
    public async Task RollAsync(CancellationToken cancellationToken = default)
    {
        await db.ExecuteAsync(async (unitOfWork, ct) =>
        {
            var settings = await unitOfWork.ScheduleSettings.GetOrCreateAsync(ct);
            var previousStart = settings.CycleStartDate;

            await unitOfWork.Lessons.DeleteExpiredOneTimeLessonsAsync(previousStart, ct);

            settings.CycleStartDate = settings.CycleStartDate.AddDays(ScheduleCycle.WeeksInCycle * 7);
            await unitOfWork.ScheduleSettings.UpdateAsync(settings, ct);

            await unitOfWork.SaveChangesAsync(ct);
        }, cancellationToken);

        cache.InvalidateSchedule();
    }
}
