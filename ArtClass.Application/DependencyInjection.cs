using Microsoft.Extensions.DependencyInjection;
using ArtClass.Application.Caching;
using ArtClass.Application.Data;
using ArtClass.Application.Services;

namespace ArtClass.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<IAppDataCache, AppDataCache>();
        services.AddSingleton<UnitOfWorkExecutor>();
        services.AddSingleton<IScheduleService, ScheduleService>();
        services.AddSingleton<IScheduleRollService, ScheduleRollService>();
        services.AddSingleton<IStudentService, StudentService>();
        services.AddSingleton<IGroupService, GroupService>();
        services.AddSingleton<IReferenceDataService, ReferenceDataService>();
        return services;
    }
}
