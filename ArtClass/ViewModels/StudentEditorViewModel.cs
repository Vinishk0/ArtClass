using System.Collections.ObjectModel;
using ArtClass.Application.Dtos;
using ArtClass.Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ArtClass.ViewModels;

[QueryProperty(nameof(Mode), "mode")]
[QueryProperty(nameof(StudentIdText), "studentId")]
public partial class StudentEditorViewModel(
    IStudentService studentService,
    IGroupService groupService) : ObservableObject
{
    private int _studentId;
    private bool _isEdit;
    private List<SelectableItemViewModel> _allGroups = [];

    [ObservableProperty]
    private int _currentStep = 1;

    [ObservableProperty]
    private string _pageTitle = "Новый ученик";

    [ObservableProperty]
    private string _stepIndicator = "Шаг 1 из 2";

    [ObservableProperty]
    private string _fullName = string.Empty;

    [ObservableProperty]
    private string _phone = string.Empty;

    [ObservableProperty]
    private string _ageText = string.Empty;

    [ObservableProperty]
    private string _groupSearchQuery = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public ObservableCollection<SelectableItemViewModel> FilteredGroups { get; } = [];

    public bool IsStep1 => CurrentStep == 1;
    public bool IsStep2 => CurrentStep == 2;
    public bool IsEditMode => _isEdit;

    public string StudentIdText
    {
        set
        {
            if (int.TryParse(value, out var id))
            {
                _studentId = id;
                _isEdit = true;
            }
        }
    }

    public string Mode
    {
        set
        {
            _isEdit = string.Equals(value, "edit", StringComparison.OrdinalIgnoreCase) || _studentId > 0;
            PageTitle = _isEdit ? "Редактирование ученика" : "Новый ученик";
        }
    }

    partial void OnCurrentStepChanged(int value)
    {
        StepIndicator = $"Шаг {value} из 2";
        OnPropertyChanged(nameof(IsStep1));
        OnPropertyChanged(nameof(IsStep2));
    }

    partial void OnGroupSearchQueryChanged(string value) => RefreshFilteredGroups();

    [RelayCommand]
    public async Task InitializeAsync()
    {
        if (_allGroups.Count == 0)
        {
            foreach (var group in await groupService.GetAllAsync())
            {
                _allGroups.Add(new SelectableItemViewModel(
                    group.Id,
                    FormatGroupLabel(group),
                    false));
            }
        }

        if (_isEdit && _studentId > 0)
        {
            await LoadStudentAsync();
            return;
        }

        ResetForm();
    }

    [RelayCommand]
    public void ToggleGroup(SelectableItemViewModel? group)
    {
        if (group is null)
        {
            return;
        }

        group.IsSelected = !group.IsSelected;
    }

    [RelayCommand]
    public void NextStep()
    {
        StatusMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(FullName))
        {
            StatusMessage = "Укажите ФИО";
            return;
        }

        CurrentStep = 2;
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

        if (string.IsNullOrWhiteSpace(FullName))
        {
            StatusMessage = "Укажите ФИО";
            CurrentStep = 1;
            return;
        }

        int? age = int.TryParse(AgeText, out var parsedAge) ? parsedAge : null;
        var groupIds = _allGroups.Where(g => g.IsSelected).Select(g => g.Id).ToList();

        try
        {
            IsBusy = true;
            StatusMessage = string.Empty;

            if (_isEdit)
            {
                await studentService.UpdateAsync(
                    _studentId,
                    new UpdateStudentRequest(
                        FullName,
                        string.IsNullOrWhiteSpace(Phone) ? null : Phone,
                        age,
                        groupIds));
            }
            else
            {
                await studentService.CreateAsync(new CreateStudentRequest(
                    FullName,
                    string.IsNullOrWhiteSpace(Phone) ? null : Phone,
                    age,
                    groupIds));
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
        if (!_isEdit || _studentId == 0 || IsBusy)
        {
            return;
        }

        var confirmed = await Shell.Current.DisplayAlert(
            "Удалить ученика?",
            "Данные ученика будут удалены без возможности восстановления.",
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
            await studentService.DeleteAsync(_studentId);
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

    private async Task LoadStudentAsync()
    {
        var student = await studentService.GetByIdAsync(_studentId);
        if (student is null)
        {
            StatusMessage = "Ученик не найден";
            return;
        }

        FullName = student.FullName;
        Phone = student.Phone ?? string.Empty;
        AgeText = student.Age?.ToString() ?? string.Empty;

        var enrolled = student.GroupIds.ToHashSet();
        foreach (var group in _allGroups)
        {
            group.IsSelected = enrolled.Contains(group.Id);
        }

        RefreshFilteredGroups();
        PageTitle = "Редактирование ученика";
    }

    private void ResetForm()
    {
        CurrentStep = 1;
        FullName = string.Empty;
        Phone = string.Empty;
        AgeText = string.Empty;
        GroupSearchQuery = string.Empty;
        StatusMessage = string.Empty;

        foreach (var group in _allGroups)
        {
            group.IsSelected = false;
        }

        RefreshFilteredGroups();
        PageTitle = "Новый ученик";
    }

    private void RefreshFilteredGroups()
    {
        FilteredGroups.Clear();
        var query = GroupSearchQuery.Trim();

        foreach (var group in _allGroups)
        {
            if (string.IsNullOrEmpty(query)
                || group.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
            {
                FilteredGroups.Add(group);
            }
        }
    }

    private static string FormatGroupLabel(GroupDto group)
    {
        if (group.IsRepeating)
        {
            return group.Name;
        }

        var slot = group.Slots.FirstOrDefault();
        return slot?.SpecificDate is not null
            ? $"✦ {group.Name} ({slot.SpecificDate:dd.MM})"
            : $"✦ {group.Name}";
    }
}
