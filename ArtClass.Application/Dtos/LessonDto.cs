namespace ArtClass.Application.Dtos;

public sealed record LessonDto(
    int Id,
    int StudyGroupId,
    string StudyGroupName,
    string StudyGroupColor,
    string TeacherName,
    string SubjectName,
    string ClassroomName,
    DayOfWeek DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime,
    string? Notes,
    bool IsRepeating,
    int? CycleWeek,
    DateOnly? SpecificDate);
