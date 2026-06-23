using System.Collections.ObjectModel;
using ArtClass.Application.Caching;
using ArtClass.Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ArtClass.ViewModels;

public partial class GroupsViewModel(IGroupService groupService, IAppDataCache cache) : ObservableObject
{
    private List<GroupListItemViewModel> _allGroups = [];
    private CancellationTokenSource? _searchCts;
    private long _loadedVersion = -1;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private ObservableCollection<GroupListItemViewModel> _groups = [];

    partial void OnSearchQueryChanged(string value) => DebounceFilter();

    [RelayCommand]
    public async Task EnsureLoadedAsync()
    {
        if (_loadedVersion == cache.Version && _allGroups.Count > 0)
        {
            return;
        }

        await LoadAsync();
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            var groups = await groupService.GetAllAsync();
            _allGroups = groups.Select(GroupListItemViewModel.FromDto).ToList();
            _loadedVersion = cache.Version;
            ApplyFilter();
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task CreateGroupAsync() =>
        await Shell.Current.GoToAsync("GroupEditorPage?mode=create");

    [RelayCommand]
    public async Task CreateExtraAsync() =>
        await Shell.Current.GoToAsync("GroupEditorPage?mode=extra");

    [RelayCommand]
    public async Task OpenGroupAsync(GroupListItemViewModel? item)
    {
        if (item is null)
        {
            return;
        }

        _ = groupService.GetByIdAsync(item.Id);
        await Shell.Current.GoToAsync($"GroupDetailPage?groupId={item.Id}");
    }

    private void DebounceFilter()
    {
        _searchCts?.Cancel();
        _searchCts?.Dispose();
        var cts = new CancellationTokenSource();
        _searchCts = cts;
        _ = ApplyFilterDebouncedAsync(cts.Token);
    }

    private async Task ApplyFilterDebouncedAsync(CancellationToken token)
    {
        try
        {
            await Task.Delay(200, token);
            ApplyFilter();
        }
        catch (OperationCanceledException)
        {
        }
    }

    private void ApplyFilter()
    {
        var query = SearchQuery.Trim();
        var filtered = string.IsNullOrEmpty(query)
            ? _allGroups
            : _allGroups.Where(g =>
                g.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                g.Subtitle.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();

        if (Groups.Count == filtered.Count
            && Groups.Zip(filtered).All(pair => pair.First.Id == pair.Second.Id))
        {
            return;
        }

        Groups = new ObservableCollection<GroupListItemViewModel>(filtered);
    }
}

public sealed class GroupListItemViewModel
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public required string Subtitle { get; init; }
    public required string TypeLabel { get; init; }
    public required bool IsRepeating { get; init; }
    public required string Color { get; init; }

    public string TypeEmoji => IsRepeating ? "🎨" : "✨";

    public static GroupListItemViewModel FromDto(Application.Dtos.GroupDto dto)
    {
        var slots = dto.Slots.Count == 0
            ? "без слотов"
            : string.Join(", ", dto.Slots.Select(s =>
                s.SpecificDate is not null
                    ? $"{s.SpecificDate:dd.MM} {s.StartTime:HH:mm}"
                    : s.CycleWeek is int week
                        ? $"{s.DayOfWeek.ToRussianShortName()} {s.StartTime:HH:mm} (н{week})"
                        : $"{s.DayOfWeek.ToRussianShortName()} {s.StartTime:HH:mm}"));

        var repeatLabel = dto.IsBiWeekly ? "2-нед." : "еженед.";

        return new()
        {
            Id = dto.Id,
            Name = dto.Name,
            Subtitle = $"{repeatLabel} · {slots} · {dto.StudentCount} уч.",
            TypeLabel = dto.IsRepeating ? "Группа" : "Мастеркласс",
            IsRepeating = dto.IsRepeating,
            Color = dto.Color,
        };
    }
}
