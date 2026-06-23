using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using ArtClass.Domain.Repositories;
using ArtClass.Infrastructure.Data;
using ArtClass.Infrastructure.Repositories;

namespace ArtClass.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string databasePath)
    {
        var settings = new DatabaseSettings { DatabasePath = databasePath };
        services.AddSingleton(settings);

        services.AddDbContext<ArtClassDbContext>(options =>
        {
            options.UseSqlite($"Data Source={databasePath}");
            options.ConfigureWarnings(warnings =>
                warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
        });

        services.AddScoped<IUnitOfWork, EfUnitOfWork>();

        return services;
    }

    public static Task InitializeDatabaseAsync(this IServiceProvider services) =>
        services.GetRequiredService<DatabaseSettings>() is { } settings
            ? DatabaseBootstrap.InitializeAsync(services, settings)
            : Task.CompletedTask;

    public static async Task ResetDatabaseAsync(this IServiceProvider services)
    {
        var settings = services.GetRequiredService<DatabaseSettings>();
        await DatabaseBootstrap.RecreateDatabaseFilesAsync(settings);
        await DatabaseBootstrap.InitializeAsync(services, settings);
    }
}
