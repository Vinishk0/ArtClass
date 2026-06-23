using System.Collections.ObjectModel;
using ArtClass.Application.Caching;
using ArtClass.Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ArtClass.ViewModels;

public partial class StudentsViewModel(IStudentService studentService, IAppDataCache cache) : ObservableObject
{
    private List<StudentListItemViewModel> _allStudents = [];
    private CancellationTokenSource? _searchCts;
    private long _loadedVersion = -1;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private ObservableCollection<StudentListItemViewModel> _students = [];

    partial void OnSearchQueryChanged(string value) => DebounceFilter();

    [RelayCommand]
    public async Task EnsureLoadedAsync()
    {
        if (_loadedVersion == cache.Version && _allStudents.Count > 0)
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
            var students = await studentService.GetAllAsync();
            _allStudents = students.Select(StudentListItemViewModel.FromDto).ToList();
            _loadedVersion = cache.Version;
            ApplyFilter();
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task AddStudentAsync() =>
        await Shell.Current.GoToAsync("StudentEditorPage?mode=create");

    [RelayCommand]
    public async Task OpenStudentAsync(StudentListItemViewModel? item)
    {
        if (item is null)
        {
            return;
        }

        _ = studentService.GetByIdAsync(item.Id);
        await Shell.Current.GoToAsync($"StudentDetailPage?studentId={item.Id}");
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
            ? _allStudents
            : _allStudents.Where(s =>
                s.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                s.Subtitle.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();

        if (Students.Count == filtered.Count
            && Students.Zip(filtered).All(pair => pair.First.Id == pair.Second.Id))
        {
            return;
        }

        Students = new ObservableCollection<StudentListItemViewModel>(filtered);
    }
}

public sealed class StudentListItemViewModel
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public required string Subtitle { get; init; }
    public required string Initials { get; init; }

    public static StudentListItemViewModel FromDto(Application.Dtos.StudentDto dto)
    {
        var groups = dto.GroupNames.Count == 0
            ? "без групп"
            : string.Join(", ", dto.GroupNames);

        var name = dto.FullName;
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var initials = parts.Length >= 2
            ? $"{parts[0][0]}{parts[1][0]}".ToUpperInvariant()
            : name.Length > 0 ? name[0].ToString().ToUpperInvariant() : "?";

        return new()
        {
            Id = dto.Id,
            Name = name,
            Subtitle = $"{dto.Age?.ToString() ?? "—"} лет · {groups}",
            Initials = initials,
        };
    }
}
