using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ArtClass.Infrastructure.Data;

public static class DatabaseBootstrap
{
    public static async Task InitializeAsync(
        IServiceProvider services,
        DatabaseSettings settings,
        CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(settings.Directory);

        Debug.WriteLine($"[ArtClass] Database path: {settings.DatabasePath}");

        try
        {
            await InitializeCoreAsync(services, cancellationToken);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ArtClass] Database init failed: {FormatException(ex)}");
            await RecreateDatabaseFilesAsync(settings);
            await InitializeCoreAsync(services, cancellationToken);
            Debug.WriteLine("[ArtClass] Database recreated and initialized successfully.");
        }
    }

    private static async Task InitializeCoreAsync(
        IServiceProvider services,
        CancellationToken cancellationToken)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ArtClassDbContext>();
        await DatabaseInitializer.InitializeAsync(context, cancellationToken);
    }

    public static async Task RecreateDatabaseFilesAsync(DatabaseSettings settings)
    {
        await Task.Run(() =>
        {
            foreach (var path in GetDatabaseFiles(settings.DatabasePath))
            {
                if (!File.Exists(path))
                {
                    continue;
                }

                Debug.WriteLine($"[ArtClass] Deleting database file: {path}");
                File.Delete(path);
            }
        });
    }

    public static IEnumerable<string> GetDatabaseFiles(string databasePath)
    {
        yield return databasePath;
        yield return databasePath + "-wal";
        yield return databasePath + "-shm";
    }

    public static string FormatException(Exception ex)
    {
        var messages = new List<string>();
        for (var current = ex; current is not null; current = current.InnerException)
        {
            messages.Add(current.Message);
        }

        return string.Join(" -> ", messages);
    }
}
