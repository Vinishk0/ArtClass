using Microsoft.Extensions.Logging;
using ArtClass.Application;
using ArtClass.Infrastructure;
using ArtClass.ViewModels;
using ArtClass.Views;

namespace ArtClass;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "artclass.db");

        builder.Services
            .AddApplication()
            .AddInfrastructure(dbPath);

        builder.Services.AddTransient<ScheduleViewModel>();
        builder.Services.AddTransient<SchedulePage>();
        builder.Services.AddSingleton<AppShell>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();
        app.Services.InitializeDatabaseAsync().GetAwaiter().GetResult();

        return app;
    }
}
