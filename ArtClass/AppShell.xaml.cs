using ArtClass.Views;

namespace ArtClass;

public partial class AppShell : Shell
{
    public AppShell(IServiceProvider services)
    {
        InitializeComponent();

        var tabBar = new TabBar
        {
            Items =
            {
                CreateTab(services.GetRequiredService<CalendarPage>(), "Календарь", "CalendarPage"),
                CreateTab(services.GetRequiredService<GroupsPage>(), "Группы", "GroupsPage"),
                CreateTab(services.GetRequiredService<StudentsPage>(), "Ученики", "StudentsPage"),
                // CreateTab(services.GetRequiredService<SettingsPage>(), "Ещё", "SettingsPage"),
            },
        };

        Items.Add(tabBar);

        Routing.RegisterRoute(nameof(DayDetailPage), typeof(DayDetailPage));
        Routing.RegisterRoute(nameof(GroupDetailPage), typeof(GroupDetailPage));
        Routing.RegisterRoute(nameof(GroupEditorPage), typeof(GroupEditorPage));
        Routing.RegisterRoute(nameof(StudentDetailPage), typeof(StudentDetailPage));
        Routing.RegisterRoute(nameof(StudentEditorPage), typeof(StudentEditorPage));
    }

    private static ShellContent CreateTab(Page page, string title, string route) =>
        new()
        {
            Title = title,
            Route = route,
            Content = page,
        };
}
