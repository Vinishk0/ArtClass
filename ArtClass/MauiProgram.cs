using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using ArtClass.Application;
using ArtClass.Infrastructure;
using ArtClass.Infrastructure.Data;
using ArtClass.Services;
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
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "artclass.db");

        builder.Services
            .AddApplication()
            .AddInfrastructure(dbPath);

        builder.Services.AddSingleton<CalendarViewModel>();
        builder.Services.AddSingleton<CalendarPage>();
        builder.Services.AddTransient<DayDetailViewModel>();
        builder.Services.AddTransient<DayDetailPage>();
        builder.Services.AddSingleton<GroupsViewModel>();
        builder.Services.AddSingleton<GroupsPage>();
        builder.Services.AddTransient<GroupDetailViewModel>();
        builder.Services.AddTransient<GroupDetailPage>();
        builder.Services.AddTransient<GroupEditorViewModel>();
        builder.Services.AddTransient<GroupEditorPage>();
        builder.Services.AddSingleton<StudentsViewModel>();
        builder.Services.AddSingleton<StudentsPage>();
        builder.Services.AddTransient<StudentDetailViewModel>();
        builder.Services.AddTransient<StudentDetailPage>();
        builder.Services.AddTransient<StudentEditorViewModel>();
        builder.Services.AddTransient<StudentEditorPage>();
        builder.Services.AddSingleton<SettingsViewModel>();
        builder.Services.AddSingleton<SettingsPage>();
        builder.Services.AddSingleton<AppShell>();
        builder.Services.AddSingleton<TabBootstrapService>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

#if ANDROID
        builder.ConfigureMauiHandlers(handlers =>
        {
            Microsoft.Maui.Handlers.ButtonHandler.Mapper.AppendToMapping("NoPressHighlight", (handler, _) =>
            {
                if (handler.PlatformView is Google.Android.Material.Button.MaterialButton materialButton)
                {
                    materialButton.StateListAnimator = null;
                    materialButton.RippleColor = Android.Content.Res.ColorStateList.ValueOf(
                        Android.Graphics.Color.Transparent);
                }
            });
        });
#endif

        var app = builder.Build();

        try
        {
            app.Services.InitializeDatabaseAsync().GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"[ArtClass] Database startup error: {DatabaseBootstrap.FormatException(ex)}");
            throw;
        }

        return app;
    }
}
