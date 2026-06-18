using Microsoft.Extensions.DependencyInjection;
using Schedule.Application.Services;

namespace Schedule.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IScheduleService, ScheduleService>();
        return services;
    }
}
