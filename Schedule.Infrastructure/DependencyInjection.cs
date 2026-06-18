using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Schedule.Domain.Repositories;
using Schedule.Infrastructure.Data;
using Schedule.Infrastructure.Repositories;

namespace Schedule.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string databasePath)
    {
        services.AddDbContext<ScheduleDbContext>(options =>
            options.UseSqlite($"Data Source={databasePath}"));

        services.AddScoped<IUnitOfWork, EfUnitOfWork>();

        return services;
    }

    public static async Task InitializeDatabaseAsync(this IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ScheduleDbContext>();
        await DatabaseInitializer.InitializeAsync(context);
    }
}
