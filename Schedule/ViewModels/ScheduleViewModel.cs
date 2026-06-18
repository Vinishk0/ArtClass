using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Schedule.Application.Dtos;
using Schedule.Application.Services;

namespace Schedule.ViewModels;

public partial class ScheduleViewModel(IScheduleService scheduleService) : ObservableObject
{
    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public IList<LessonItemViewModel> Lessons { get; } = [];

    [RelayCommand]
    public async Task LoadScheduleAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = string.Empty;

            var lessons = await scheduleService.GetWeekScheduleAsync();

            Lessons.Clear();
            
            foreach (var lesson in lessons)
            {
                Lessons.Add(LessonItemViewModel.FromDto(lesson));
            }

            StatusMessage = Lessons.Count == 0
                ? "Расписание пусто"
                : $"Занятий на неделе: {Lessons.Count}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка загрузки: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}

public sealed class LessonItemViewModel
{
    public required string DayName { get; init; }
    public required string TimeRange { get; init; }
    public required string Title { get; init; }
    public required string Details { get; init; }

    public static LessonItemViewModel FromDto(LessonDto dto) =>
        new()
        {
            DayName = dto.DayOfWeek.ToRussianName(),
            TimeRange = $"{dto.StartTime:HH:mm}–{dto.EndTime:HH:mm}",
            Title = $"{dto.StudyGroupName} · {dto.SubjectName}",
            Details = $"{dto.TeacherName} · {dto.ClassroomName}",
        };
}
