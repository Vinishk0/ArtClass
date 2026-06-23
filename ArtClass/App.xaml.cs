namespace ArtClass;

public partial class App : Microsoft.Maui.Controls.Application
{
    private readonly IServiceProvider _services;

    public App(IServiceProvider services)
    {
        InitializeComponent();
        _services = services;
        RegisterExceptionLogging();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(_services.GetRequiredService<AppShell>());
        _ = WarmUpInBackgroundAsync();
        return window;
    }

    private async Task WarmUpInBackgroundAsync()
    {
        try
        {
            await _services.GetRequiredService<Services.TabBootstrapService>().WarmUpAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"[ArtClass] Startup warm-up error: {Infrastructure.Data.DatabaseBootstrap.FormatException(ex)}");
        }
    }

    private static void RegisterExceptionLogging()
    {
        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        {
            if (args.ExceptionObject is Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[ArtClass] Unhandled: {Infrastructure.Data.DatabaseBootstrap.FormatException(ex)}");
            }
        };

        TaskScheduler.UnobservedTaskException += (_, args) =>
        {
            System.Diagnostics.Debug.WriteLine(
                $"[ArtClass] Task error: {Infrastructure.Data.DatabaseBootstrap.FormatException(args.Exception)}");
            args.SetObserved();
        };

#if ANDROID
        Android.Runtime.AndroidEnvironment.UnhandledExceptionRaiser += (_, args) =>
        {
            System.Diagnostics.Debug.WriteLine(
                $"[ArtClass] Android: {Infrastructure.Data.DatabaseBootstrap.FormatException(args.Exception)}");
            args.Handled = false;
        };
#endif
    }
}
