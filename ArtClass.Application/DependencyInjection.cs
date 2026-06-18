using Microsoft.Extensions.DependencyInjection;
using ArtClass.Application.Services;

namespace ArtClass.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IScheduleService, ScheduleService>();
        return services;
    }
}
