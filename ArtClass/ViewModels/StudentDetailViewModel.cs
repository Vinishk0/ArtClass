using System.Collections.ObjectModel;
using ArtClass.Application.Caching;
using ArtClass.Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ArtClass.ViewModels;

[QueryProperty(nameof(StudentIdText), "studentId")]
public partial class StudentDetailViewModel(IStudentService studentService, IAppDataCache cache) : ObservableObject
{
    private int _studentId;
    private int _loadedStudentId;
    private long _loadedVersion = -1;

    [ObservableProperty]
    private string _fullName = string.Empty;

    [ObservableProperty]
    private string _phone = string.Empty;

    [ObservableProperty]
    private string _age = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    public string Initials
    {
        get
        {
            var parts = FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return parts.Length >= 2
                ? $"{parts[0][0]}{parts[1][0]}".ToUpperInvariant()
                : FullName.Length > 0 ? FullName[0].ToString().ToUpperInvariant() : "?";
        }
    }

    public ObservableCollection<string> Groups { get; } = [];

    public string StudentIdText
    {
        set
        {
            if (int.TryParse(value, out var id) && id != _studentId)
            {
                _studentId = id;
                _loadedStudentId = 0;
            }
        }
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        if (_studentId == 0)
        {
            return;
        }

        if (_loadedStudentId == _studentId && _loadedVersion == cache.Version)
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
            var student = await studentService.GetByIdAsync(_studentId);
            if (student is null)
            {
                return;
            }

            FullName = student.FullName;
            Phone = student.Phone ?? "—";
            Age = student.Age?.ToString() ?? "—";
            OnPropertyChanged(nameof(Initials));

            Groups.Clear();
            foreach (var group in student.GroupNames)
            {
                Groups.Add(group);
            }

            _loadedStudentId = _studentId;
            _loadedVersion = cache.Version;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task EditAsync() =>
        await Shell.Current.GoToAsync($"StudentEditorPage?mode=edit&studentId={_studentId}");

    [RelayCommand]
    public async Task DeleteAsync()
    {
        if (IsBusy || _studentId == 0)
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
            await studentService.DeleteAsync(_studentId);
            await Shell.Current.GoToAsync("..");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
