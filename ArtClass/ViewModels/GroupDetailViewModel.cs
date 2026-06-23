using System.Collections.ObjectModel;
using ArtClass.Application.Caching;
using ArtClass.Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ArtClass.ViewModels;

[QueryProperty(nameof(GroupIdText), "groupId")]
public partial class GroupDetailViewModel(
    IGroupService groupService,
    IStudentService studentService,
    IAppDataCache cache) : ObservableObject
{
    private int _groupId;
    private int _loadedGroupId;
    private long _loadedVersion = -1;
    private bool _isMasterclass;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private string _typeLabel = string.Empty;

    [ObservableProperty]
    private string _color = "#512BD4";

    [ObservableProperty]
    private bool _canEdit;

    [ObservableProperty]
    private bool _isBusy;

    public ObservableCollection<string> Slots { get; } = [];
    public ObservableCollection<string> Students { get; } = [];

    public bool IsMasterclass => _isMasterclass;

    public string GroupIdText
    {
        set
        {
            if (int.TryParse(value, out var id) && id != _groupId)
            {
                _groupId = id;
                _loadedGroupId = 0;
            }
        }
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        if (_groupId == 0)
        {
            return;
        }

        if (_loadedGroupId == _groupId && _loadedVersion == cache.Version)
        {
            return;
        }

        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            var group = await groupService.GetByIdAsync(_groupId);
            if (group is null)
            {
                return;
            }

            Name = group.Name;
            Description = string.IsNullOrWhiteSpace(group.Description) ? "—" : group.Description;
            TypeLabel = group.IsRepeating ? "Группа" : "Мастеркласс";
            Color = group.Color;
            CanEdit = true;
            _isMasterclass = !group.IsRepeating;
            OnPropertyChanged(nameof(IsMasterclass));

            Slots.Clear();
            foreach (var slot in group.Slots)
            {
                var timeLabel = $"{slot.StartTime:HH:mm}–{slot.EndTime:HH:mm}";
                var label = slot.SpecificDate is not null
                    ? $"{slot.SpecificDate:dd.MM.yyyy} {timeLabel}"
                    : slot.CycleWeek is int week
                        ? $"{slot.DayOfWeek.ToRussianShortName()} {timeLabel}, нед. {week}"
                        : $"{slot.DayOfWeek.ToRussianShortName()} {timeLabel}";
                Slots.Add(label);
            }

            if (Slots.Count == 0)
            {
                Slots.Add("Расписание не задано");
            }

            Students.Clear();
            if (group.StudentIds.Count == 0)
            {
                Students.Add("Нет учеников");
            }
            else
            {
                var allStudents = await studentService.GetAllAsync();
                var namesById = allStudents.ToDictionary(s => s.Id, s => s.FullName);
                foreach (var studentId in group.StudentIds.OrderBy(id => namesById.GetValueOrDefault(id, string.Empty)))
                {
                    if (namesById.TryGetValue(studentId, out var name))
                    {
                        Students.Add(name);
                    }
                }
            }

            _loadedGroupId = _groupId;
            _loadedVersion = cache.Version;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task EditAsync()
    {
        if (!CanEdit || _groupId == 0)
        {
            return;
        }

        var group = await groupService.GetByIdAsync(_groupId);
        if (group is null)
        {
            return;
        }

        var mode = group.IsRepeating ? "edit" : "extra";
        await Shell.Current.GoToAsync($"GroupEditorPage?mode={mode}&groupId={_groupId}");
    }

    [RelayCommand]
    public async Task DeleteAsync()
    {
        if (!_isMasterclass || IsBusy || _groupId == 0)
        {
            return;
        }

        var confirmed = await Shell.Current.DisplayAlert(
            "Удалить мастеркласс?",
            "Мастеркласс будет удалён без возможности восстановления.",
            "Удалить",
            "Отмена");

        if (!confirmed)
        {
            return;
        }

        try
        {
            IsBusy = true;
            await groupService.DeleteAsync(_groupId);
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Ошибка", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
