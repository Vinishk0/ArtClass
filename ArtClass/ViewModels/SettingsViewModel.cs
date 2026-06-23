using ArtClass.Application.Caching;
using ArtClass.Application.Services;
using ArtClass.Infrastructure;
using ArtClass.Infrastructure.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ArtClass.ViewModels;

public partial class SettingsViewModel(
    IScheduleService scheduleService,
    IScheduleRollService scheduleRollService,
    DatabaseSettings databaseSettings,
    IAppDataCache cache,
    IServiceProvider services) : ObservableObject
{
    private long _loadedVersion = -1;

    [ObservableProperty]
    private string _cycleInfo = string.Empty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    public string DatabasePath => databaseSettings.DatabasePath;

    public string DatabaseHint =>
        $"Android: /data/data/com.vinishk0.artclass/files/artclass.db\nПапка: {databaseSettings.Directory}";

    [RelayCommand]
    public async Task EnsureLoadedAsync()
    {
        if (_loadedVersion == cache.Version && !string.IsNullOrEmpty(CycleInfo))
        {
            return;
        }

        await LoadAsync();
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        try
        {
            var cycle = await scheduleService.GetCurrentCycleAsync();
            CycleInfo =
                $"Текущий цикл: {cycle.CycleStartDate:dd.MM.yyyy} – {cycle.CycleEndDate:dd.MM.yyyy}\nСейчас неделя {cycle.CurrentWeek} из 2";
            _loadedVersion = cache.Version;
        }
        catch (Exception ex)
        {
            CycleInfo = $"Не удалось загрузить цикл: {ex.Message}";
        }
    }

    [RelayCommand]
    public async Task RollScheduleAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = string.Empty;
            await scheduleRollService.RollAsync();
            await LoadAsync();
            StatusMessage = "Цикл сдвинут на 2 недели, просроченные разовые уроки удалены";
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task ResetDatabaseAsync()
    {
        if (IsBusy)
        {
            return;
        }

        var confirmed = await Shell.Current.DisplayAlert(
            "Сбросить базу?",
            "Все группы, ученики и расписание будут удалены. Приложение создаст базу заново с демо-данными.",
            "Сбросить",
            "Отмена");

        if (!confirmed)
        {
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = string.Empty;
            await services.ResetDatabaseAsync();
            await LoadAsync();
            StatusMessage = "База данных пересоздана";
        }
        catch (Exception ex)
        {
            StatusMessage = DatabaseBootstrap.FormatException(ex);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
