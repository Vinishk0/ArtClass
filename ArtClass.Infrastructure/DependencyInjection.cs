using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ArtClass.Domain.Repositories;
using ArtClass.Infrastructure.Data;
using ArtClass.Infrastructure.Repositories;

namespace ArtClass.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string databasePath)
    {
        services.AddDbContext<ArtClassDbContext>(options =>
            options.UseSqlite($"Data Source={databasePath}"));

        services.AddScoped<IUnitOfWork, EfUnitOfWork>();

        return services;
    }

    public static async Task InitializeDatabaseAsync(this IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ArtClassDbContext>();
        await DatabaseInitializer.InitializeAsync(context);
    }
}
