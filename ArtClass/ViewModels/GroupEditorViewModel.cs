using System.Collections.ObjectModel;
using ArtClass.Application;
using ArtClass.Application.Dtos;
using ArtClass.Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ArtClass.ViewModels;

[QueryProperty(nameof(Mode), "mode")]
[QueryProperty(nameof(GroupIdText), "groupId")]
public partial class GroupEditorViewModel(
    IGroupService groupService,
    IStudentService studentService) : ObservableObject
{
    private int _groupId;
    private bool _isExtra;
    private bool _isEdit;
    private List<SelectableItemViewModel> _allStudents = [];

    [ObservableProperty]
    private int _currentStep = 1;

    [ObservableProperty]
    private string _pageTitle = "Новая группа";

    [ObservableProperty]
    private string _stepIndicator = "Шаг 1 из 3";

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private string _selectedColor = GroupColors.Default;

    [ObservableProperty]
    private DateTime _lessonDate = DateTime.Today;

    [ObservableProperty]
    private TimeSpan _startTime = new(10, 0, 0);

    [ObservableProperty]
    private TimeSpan _endTime = new(11, 30, 0);

    [ObservableProperty]
    private int _selectedDayIndex;

    [ObservableProperty]
    private int _selectedCycleWeekIndex;

    [ObservableProperty]
    private int _selectedRepeatModeIndex;

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public ObservableCollection<string> Days { get; } = new(DayOfWeekOptions.All.Select(d => d.ToRussianName()));
    public ObservableCollection<string> RepeatModes { get; } = ["Каждую неделю", "Раз в 2 недели"];
    public ObservableCollection<string> CycleWeeks { get; } = ["Неделя 1", "Неделя 2"];
    public ObservableCollection<SlotEditorItem> Slots { get; } = [];
    public ObservableCollection<SelectableItemViewModel> FilteredStudents { get; } = [];
    public ObservableCollection<ColorSwatchItem> ColorSwatches { get; } = new(
        GroupColors.Palette.Select(hex => new ColorSwatchItem(hex)));

    public bool IsExtraMode => _isExtra;
    public bool IsEditMode => _isEdit;
    public bool IsStep1 => CurrentStep == 1;
    public bool IsStep2 => CurrentStep == 2;
    public bool IsStep3 => CurrentStep == 3;
    public bool IsRepeatingGroupFlow => !_isExtra;
    public bool IsBiWeekly => SelectedRepeatModeIndex == 1;
    public string DeleteButtonText => _isExtra ? "Удалить мастеркласс" : "Удалить группу";

    public string GroupIdText
    {
        set
        {
            if (int.TryParse(value, out var id))
            {
                _groupId = id;
                _isEdit = true;
            }
        }
    }

    public string Mode
    {
        set
        {
            _isExtra = string.Equals(value, "extra", StringComparison.OrdinalIgnoreCase);
            _isEdit = string.Equals(value, "edit", StringComparison.OrdinalIgnoreCase) || _groupId > 0;

            PageTitle = _isExtra
                ? _isEdit ? "Редактирование мастеркласса" : "Новый мастеркласс"
                : _isEdit
                    ? "Редактирование группы"
                    : "Новая группа";

            OnPropertyChanged(nameof(IsExtraMode));
            OnPropertyChanged(nameof(IsEditMode));
            OnPropertyChanged(nameof(IsRepeatingGroupFlow));
            OnPropertyChanged(nameof(DeleteButtonText));
        }
    }

    partial void OnCurrentStepChanged(int value)
    {
        StepIndicator = _isExtra ? $"Шаг {value} из 3" : $"Шаг {value} из 3";
        OnPropertyChanged(nameof(IsStep1));
        OnPropertyChanged(nameof(IsStep2));
        OnPropertyChanged(nameof(IsStep3));
    }

    partial void OnSearchQueryChanged(string value) => RefreshFilteredStudents();

    partial void OnSelectedColorChanged(string value) => UpdateColorSelection();

    partial void OnSelectedRepeatModeIndexChanged(int value)
    {
        OnPropertyChanged(nameof(IsBiWeekly));
    }

    [RelayCommand]
    public void SelectColor(ColorSwatchItem? swatch)
    {
        if (swatch is null)
        {
            return;
        }

        SelectedColor = swatch.Hex;
    }

    [RelayCommand]
    public async Task InitializeAsync()
    {
        if (_allStudents.Count == 0)
        {
            foreach (var student in await studentService.GetAllAsync())
            {
                _allStudents.Add(new SelectableItemViewModel(student.Id, student.FullName, false));
            }
        }

        if (_isEdit && _groupId > 0)
        {
            await LoadGroupAsync();
            return;
        }

        ResetForm();
    }

    private void UpdateColorSelection()
    {
        foreach (var swatch in ColorSwatches)
        {
            swatch.IsSelected = swatch.Hex == SelectedColor;
        }
    }

    [RelayCommand]
    public void AddSlot()
    {
        var day = DayOfWeekOptions.All[SelectedDayIndex];
        var timeLabel = $"{StartTime:hh\\:mm}–{EndTime:hh\\:mm}";
        var label = IsBiWeekly
            ? $"{day.ToRussianShortName()} {timeLabel}, нед. {SelectedCycleWeekIndex + 1}"
            : $"{day.ToRussianShortName()} {timeLabel}";

        Slots.Add(new SlotEditorItem
        {
            Label = label,
            DayOfWeek = day,
            CycleWeek = IsBiWeekly ? SelectedCycleWeekIndex + 1 : null,
            StartTime = TimeOnly.FromTimeSpan(StartTime),
            EndTime = TimeOnly.FromTimeSpan(EndTime),
        });
    }

    [RelayCommand]
    public void RemoveSlot(SlotEditorItem? slot)
    {
        if (slot is not null)
        {
            Slots.Remove(slot);
        }
    }

    [RelayCommand]
    public void ToggleStudent(SelectableItemViewModel? student)
    {
        if (student is null)
        {
            return;
        }

        student.IsSelected = !student.IsSelected;
    }

    [RelayCommand]
    public void NextStep()
    {
        StatusMessage = string.Empty;

        if (CurrentStep == 1)
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                StatusMessage = "Укажите название";
                return;
            }

            CurrentStep = 2;
            return;
        }

        if (CurrentStep == 2)
        {
            if (_isExtra)
            {
                CurrentStep = 3;
                return;
            }

            if (Slots.Count == 0)
            {
                StatusMessage = "Добавьте хотя бы один слот расписания";
                return;
            }

            CurrentStep = 3;
        }
    }

    [RelayCommand]
    public void BackStep()
    {
        StatusMessage = string.Empty;
        if (CurrentStep > 1)
        {
            CurrentStep--;
        }
    }

    [RelayCommand]
    public async Task SaveAsync()
    {
        if (IsBusy)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(Name))
        {
            StatusMessage = "Укажите название";
            CurrentStep = 1;
            return;
        }

        if (!_isExtra && Slots.Count == 0)
        {
            StatusMessage = "Добавьте хотя бы один слот расписания";
            CurrentStep = 2;
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = string.Empty;

            var studentIds = _allStudents.Where(s => s.IsSelected).Select(s => s.Id).ToList();

            if (_isExtra)
            {
                if (_isEdit)
                {
                    await groupService.UpdateExtraLessonAsync(
                        _groupId,
                        new UpdateExtraLessonRequest(
                            Name,
                            DateOnly.FromDateTime(LessonDate),
                            TimeOnly.FromTimeSpan(StartTime),
                            TimeOnly.FromTimeSpan(EndTime),
                            Description,
                            SelectedColor,
                            studentIds));
                }
                else
                {
                    await groupService.CreateExtraLessonAsync(new CreateExtraLessonRequest(
                        Name,
                        DateOnly.FromDateTime(LessonDate),
                        TimeOnly.FromTimeSpan(StartTime),
                        TimeOnly.FromTimeSpan(EndTime),
                        Description,
                        SelectedColor,
                        studentIds));
                }
            }
            else if (_isEdit)
            {
                await groupService.UpdateRepeatingGroupAsync(
                    _groupId,
                    new UpdateGroupRequest(
                        Name,
                        Description,
                        SelectedColor,
                        IsBiWeekly,
                        Slots.Select(s => new GroupSlotInput(s.DayOfWeek, s.StartTime, s.EndTime, s.CycleWeek)).ToList(),
                        studentIds));
            }
            else
            {
                await groupService.CreateRepeatingGroupAsync(new CreateGroupRequest(
                    Name,
                    Description,
                    SelectedColor,
                    IsBiWeekly,
                    Slots.Select(s => new GroupSlotInput(s.DayOfWeek, s.StartTime, s.EndTime, s.CycleWeek)).ToList(),
                    studentIds));
            }

            await Shell.Current.GoToAsync("..");
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
    public async Task DeleteAsync()
    {
        if (!_isEdit || _groupId == 0 || IsBusy)
        {
            return;
        }

        var confirmed = await Shell.Current.DisplayAlert(
            _isExtra ? "Удалить мастеркласс?" : "Удалить группу?",
            _isExtra
                ? "Мастеркласс будет удалён без возможности восстановления."
                : "Группа и её расписание будут удалены без возможности восстановления.",
            "Удалить",
            "Отмена");

        if (!confirmed)
        {
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = string.Empty;
            await groupService.DeleteAsync(_groupId);
            await Shell.Current.GoToAsync("../..");
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

    private async Task LoadGroupAsync()
    {
        var group = await groupService.GetByIdAsync(_groupId);
        if (group is null)
        {
            StatusMessage = "Не найдено";
            return;
        }

        if (!group.IsRepeating)
        {
            _isExtra = true;
            OnPropertyChanged(nameof(IsExtraMode));
            OnPropertyChanged(nameof(IsRepeatingGroupFlow));

            Name = group.Name;
            Description = group.Description is "Разовое занятие" ? string.Empty : group.Description ?? string.Empty;
            SelectedColor = group.Color;
            UpdateColorSelection();

            var slot = group.Slots.FirstOrDefault();
            if (slot?.SpecificDate is not null)
            {
                LessonDate = slot.SpecificDate.Value.ToDateTime(TimeOnly.MinValue);
                StartTime = slot.StartTime.ToTimeSpan();
                EndTime = slot.EndTime.ToTimeSpan();
            }

            ApplyStudentSelection(group.StudentIds);
            RefreshFilteredStudents();
            PageTitle = "Редактирование мастеркласса";
            return;
        }

        Name = group.Name;
        Description = group.Description ?? string.Empty;
        SelectedColor = group.Color;
        SelectedRepeatModeIndex = group.IsBiWeekly ? 1 : 0;
        OnPropertyChanged(nameof(IsBiWeekly));
        UpdateColorSelection();
        Slots.Clear();

        foreach (var slot in group.Slots.Where(s => s.SpecificDate is null))
        {
            var timeLabel = $"{slot.StartTime:HH:mm}–{slot.EndTime:HH:mm}";
            var label = slot.CycleWeek is int week
                ? $"{slot.DayOfWeek.ToRussianShortName()} {timeLabel}, нед. {week}"
                : $"{slot.DayOfWeek.ToRussianShortName()} {timeLabel}";

            Slots.Add(new SlotEditorItem
            {
                Label = label,
                DayOfWeek = slot.DayOfWeek,
                CycleWeek = slot.CycleWeek,
                StartTime = slot.StartTime,
                EndTime = slot.EndTime,
            });
        }

        ApplyStudentSelection(group.StudentIds);
        RefreshFilteredStudents();
        PageTitle = "Редактирование группы";
    }

    private void ApplyStudentSelection(IReadOnlyList<int> studentIds)
    {
        var enrolled = studentIds.ToHashSet();
        foreach (var student in _allStudents)
        {
            student.IsSelected = enrolled.Contains(student.Id);
        }
    }

    private void ResetForm()
    {
        CurrentStep = 1;
        Name = string.Empty;
        Description = string.Empty;
        SelectedColor = GroupColors.Default;
        SelectedRepeatModeIndex = 0;
        OnPropertyChanged(nameof(IsBiWeekly));
        UpdateColorSelection();
        SearchQuery = string.Empty;
        Slots.Clear();
        LessonDate = DateTime.Today;
        StartTime = new TimeSpan(10, 0, 0);
        EndTime = new TimeSpan(11, 30, 0);
        StatusMessage = string.Empty;

        foreach (var student in _allStudents)
        {
            student.IsSelected = false;
        }

        RefreshFilteredStudents();

        PageTitle = _isExtra ? "Новый мастеркласс" : "Новая группа";
    }

    private void RefreshFilteredStudents()
    {
        FilteredStudents.Clear();
        var query = SearchQuery.Trim();

        foreach (var student in _allStudents)
        {
            if (string.IsNullOrEmpty(query)
                || student.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
            {
                FilteredStudents.Add(student);
            }
        }
    }
}

public sealed class SlotEditorItem
{
    public required string Label { get; init; }
    public required DayOfWeek DayOfWeek { get; init; }
    public int? CycleWeek { get; init; }
    public required TimeOnly StartTime { get; init; }
    public required TimeOnly EndTime { get; init; }
}

public partial class SelectableItemViewModel(int id, string name, bool isSelected) : ObservableObject
{
    public int Id { get; } = id;
    public string Name { get; } = name;

    [ObservableProperty]
    private bool _isSelected = isSelected;

    public string SelectionMark => IsSelected ? "[x]" : "[ ]";

    partial void OnIsSelectedChanged(bool value) => OnPropertyChanged(nameof(SelectionMark));
}

public partial class ColorSwatchItem(string hex) : ObservableObject
{
    public string Hex { get; } = hex;

    [ObservableProperty]
    private bool _isSelected;

    public Color StrokeColor => IsSelected
        ? (Microsoft.Maui.Controls.Application.Current?.RequestedTheme == AppTheme.Dark
            ? Color.FromArgb("#E07A5F")
            : Color.FromArgb("#C45C3E"))
        : Colors.Transparent;

    partial void OnIsSelectedChanged(bool value) => OnPropertyChanged(nameof(StrokeColor));
}
