using Schedule.Domain.Common;

namespace Schedule.Domain.Entities;

public class Lesson : Entity
{
    public int StudyGroupId { get; set; }

    public StudyGroup StudyGroup { get; set; } = null!;

    public int TeacherId { get; set; }

    public Teacher Teacher { get; set; } = null!;

    public int SubjectId { get; set; }

    public Subject Subject { get; set; } = null!;

    public int ClassroomId { get; set; }

    public Classroom Classroom { get; set; } = null!;

    public DayOfWeek DayOfWeek { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public string? Notes { get; set; }
}
