using ArtClass.Application.Caching;
using ArtClass.Application.Dtos;
using ArtClass.Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ArtClass.ViewModels;

public partial class CalendarViewModel(IScheduleService scheduleService, IAppDataCache cache) : ObservableObject
{
    private CancellationTokenSource? _loadCts;
    private (int Year, int Month)? _loadedMonth;
    private long _loadedVersion = -1;

    [ObservableProperty]
    private DateOnly _displayMonth = new(
        DateOnly.FromDateTime(DateTime.Today).Year,
        DateOnly.FromDateTime(DateTime.Today).Month,
        1);

    [ObservableProperty]
    private string _monthTitle = string.Empty;

    [ObservableProperty]
    private IReadOnlyList<CalendarDayItem> _days = CreateEmptyDays();

    [RelayCommand]
    public async Task EnsureLoadedAsync()
    {
        var current = (DisplayMonth.Year, DisplayMonth.Month);
        if (_loadedVersion == cache.Version
            && _loadedMonth == current
            && Days.Any(day => day.DayNumber > 0))
        {
            return;
        }

        await LoadAsync();
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        _loadCts?.Cancel();
        _loadCts?.Dispose();
        var cts = new CancellationTokenSource();
        _loadCts = cts;
        var token = cts.Token;

        try
        {
            MonthTitle = DateFormatting.FormatMonth(DisplayMonth);

            var days = await scheduleService.GetCalendarDaysAsync(
                DisplayMonth.Year,
                DisplayMonth.Month,
                token);

            if (token.IsCancellationRequested)
            {
                return;
            }

            Days = days.Select(CalendarDayItem.FromDto).ToArray();
            _loadedMonth = (DisplayMonth.Year, DisplayMonth.Month);
            _loadedVersion = cache.Version;

            _ = PrefetchAdjacentMonthAsync(DisplayMonth.AddMonths(-1), token);
            _ = PrefetchAdjacentMonthAsync(DisplayMonth.AddMonths(1), token);
        }
        catch (OperationCanceledException)
        {
            // Быстрое листание месяцев — отменяем предыдущий запрос.
        }
    }

    [RelayCommand]
    public async Task PreviousMonthAsync()
    {
        DisplayMonth = DisplayMonth.AddMonths(-1);
        await LoadAsync();
    }

    [RelayCommand]
    public async Task NextMonthAsync()
    {
        DisplayMonth = DisplayMonth.AddMonths(1);
        await LoadAsync();
    }

    [RelayCommand]
    public async Task GoTodayAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        DisplayMonth = new DateOnly(today.Year, today.Month, 1);
        await LoadAsync();
    }

    [RelayCommand]
    public async Task OpenDayAsync(CalendarDayItem? day)
    {
        if (day is null)
        {
            return;
        }

        _ = scheduleService.GetDayScheduleByDateAsync(day.Date);
        await Shell.Current.GoToAsync($"DayDetailPage?date={day.Date:yyyy-MM-dd}");
    }

    private async Task PrefetchAdjacentMonthAsync(DateOnly month, CancellationToken token)
    {
        try
        {
            await scheduleService.GetCalendarDaysAsync(month.Year, month.Month, token);
        }
        catch (OperationCanceledException)
        {
        }
    }

    private static CalendarDayItem[] CreateEmptyDays()
    {
        var items = new CalendarDayItem[42];
        for (var i = 0; i < items.Length; i++)
        {
            items[i] = CalendarDayItem.Empty;
        }

        return items;
    }
}

public sealed class CalendarDayItem
{
    public static readonly CalendarDayItem Empty = new()
    {
        Date = default,
        DayNumber = 0,
        IsCurrentMonth = false,
        IsToday = false,
        LessonCount = 0,
        GroupColors = [],
        FillLight = Colors.Transparent,
        FillDark = Colors.Transparent,
        AccentColors = [],
    };

    public required DateOnly Date { get; init; }
    public required int DayNumber { get; init; }
    public required bool IsCurrentMonth { get; init; }
    public required bool IsToday { get; init; }
    public required int LessonCount { get; init; }
    public required IReadOnlyList<string> GroupColors { get; init; }
    public required Color FillLight { get; init; }
    public required Color FillDark { get; init; }
    public required IReadOnlyList<Color> AccentColors { get; init; }

    public bool HasLessons => LessonCount > 0;

    public double CellOpacity => IsCurrentMonth ? 1 : 0.38;

    public bool HasSamePresentationAs(CalendarDayItem other) =>
        DayNumber == other.DayNumber
        && LessonCount == other.LessonCount
        && IsToday == other.IsToday
        && Math.Abs(CellOpacity - other.CellOpacity) < 0.001
        && FillLight.ToArgbHex() == other.FillLight.ToArgbHex()
        && FillDark.ToArgbHex() == other.FillDark.ToArgbHex()
        && AccentColors.SequenceEqual(other.AccentColors, ColorComparer.Instance);

    public static CalendarDayItem FromDto(CalendarDayDto dto)
    {
        var accentColors = dto.GroupColors.Select(ParseHex).ToList();
        var (fillLight, fillDark) = BuildFill(dto.IsToday, accentColors);

        return new()
        {
            Date = dto.Date,
            DayNumber = dto.Date.Day,
            IsCurrentMonth = dto.IsCurrentMonth,
            IsToday = dto.IsToday,
            LessonCount = dto.LessonCount,
            GroupColors = dto.GroupColors,
            FillLight = fillLight,
            FillDark = fillDark,
            AccentColors = accentColors,
        };
    }

    private static (Color Light, Color Dark) BuildFill(bool isToday, IReadOnlyList<Color> accentColors)
    {
        if (accentColors.Count > 0)
        {
            var primary = accentColors[0];
            return (primary.WithAlpha(0.42f), primary.WithAlpha(0.58f));
        }

        if (isToday)
        {
            return (CalendarViewModelTodayColors.Light, CalendarViewModelTodayColors.Dark);
        }

        return (Colors.Transparent, Colors.Transparent);
    }

    private static Color ParseHex(string hex)
    {
        try
        {
            return Color.FromArgb(ArtClass.Application.GroupColors.Normalize(hex));
        }
        catch
        {
            return Color.FromArgb(ArtClass.Application.GroupColors.Default);
        }
    }

    private sealed class ColorComparer : IEqualityComparer<Color>
    {
        public static readonly ColorComparer Instance = new();

        public bool Equals(Color x, Color y) => x.ToArgbHex() == y.ToArgbHex();

        public int GetHashCode(Color obj) => obj.ToArgbHex().GetHashCode();
    }
}

internal static class CalendarViewModelTodayColors
{
    internal static readonly Color Light = Color.FromArgb("#FEF3C7");
    internal static readonly Color Dark = Color.FromArgb("#3D3520");
}
