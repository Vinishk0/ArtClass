using ArtClass.ViewModels;

namespace ArtClass.Services;

public sealed class TabBootstrapService(IServiceProvider services)
{
    public async Task WarmUpAsync()
    {
        await MainThread.InvokeOnMainThreadAsync(WarmUpPages);

        var calendar = services.GetRequiredService<CalendarViewModel>();
        await calendar.EnsureLoadedAsync();

        await Task.WhenAll(
            services.GetRequiredService<GroupsViewModel>().EnsureLoadedAsync(),
            services.GetRequiredService<StudentsViewModel>().EnsureLoadedAsync());
    }

    private void WarmUpPages()
    {
        _ = services.GetRequiredService<Views.CalendarPage>();
        _ = services.GetRequiredService<Views.GroupsPage>();
        _ = services.GetRequiredService<Views.StudentsPage>();
    }
}
