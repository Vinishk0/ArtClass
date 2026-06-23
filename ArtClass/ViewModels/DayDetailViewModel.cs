using System.Collections.ObjectModel;
using ArtClass.Application.Caching;
using ArtClass.Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ArtClass.ViewModels;

[QueryProperty(nameof(DateIso), "date")]
public partial class DayDetailViewModel(
    IScheduleService scheduleService,
    IGroupService groupService,
    IAppDataCache cache) : ObservableObject
{
    private DateOnly _date = DateOnly.FromDateTime(DateTime.Today);
    private DateOnly? _loadedDate;
    private long _loadedVersion = -1;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private ObservableCollection<LessonItemViewModel> _lessons = [];

    public bool IsEmpty => Lessons.Count == 0;

    public string DateIso
    {
        set
        {
            if (DateOnly.TryParse(value, out var parsed) && parsed != _date)
            {
                _date = parsed;
                _loadedDate = null;
            }
        }
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        if (_loadedDate == _date && _loadedVersion == cache.Version)
        {
            return;
        }

        Title = DateFormatting.FormatDay(_date);

        var lessons = await scheduleService.GetDayScheduleByDateAsync(_date);
        Lessons = new ObservableCollection<LessonItemViewModel>(
            lessons.Select(LessonItemViewModel.FromDto));
        _loadedDate = _date;
        _loadedVersion = cache.Version;
        OnPropertyChanged(nameof(IsEmpty));
    }

    [RelayCommand]
    public async Task OpenLessonAsync(LessonItemViewModel? lesson)
    {
        if (lesson is null || lesson.GroupId == 0)
        {
            return;
        }

        _ = groupService.GetByIdAsync(lesson.GroupId);
        await Shell.Current.GoToAsync($"GroupDetailPage?groupId={lesson.GroupId}");
    }
}

public sealed class LessonItemViewModel
{
    public required int GroupId { get; init; }
    public required string TimeRange { get; init; }
    public required string Title { get; init; }
    public required string Badge { get; init; }
    public required string GroupColor { get; init; }

    public static LessonItemViewModel FromDto(Application.Dtos.LessonDto dto)
    {
        var badge = dto.IsRepeating
            ? dto.CycleWeek is int week ? $"нед. {week}" : "еженед."
            : "разовый";

        return new()
        {
            GroupId = dto.StudyGroupId,
            TimeRange = $"{dto.StartTime:HH:mm}–{dto.EndTime:HH:mm}",
            Title = dto.StudyGroupName,
            Badge = badge,
            GroupColor = dto.StudyGroupColor,
        };
    }
}
