using Schedule.Views;

namespace Schedule;

public partial class AppShell : Shell
{
    public AppShell(IServiceProvider services)
    {
        InitializeComponent();

        Items.Add(new ShellContent
        {
            Title = "Расписание",
            Content = services.GetRequiredService<SchedulePage>(),
            Route = "SchedulePage",
        });
    }
}
