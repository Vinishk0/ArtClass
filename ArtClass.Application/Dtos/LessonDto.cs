namespace ArtClass.Application.Dtos;

public sealed record LessonDto(
    int Id,
    string StudyGroupName,
    string TeacherName,
    string SubjectName,
    string ClassroomName,
    DayOfWeek DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime,
    string? Notes);
