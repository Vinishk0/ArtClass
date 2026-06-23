using ArtClass.Domain.Common;

namespace ArtClass.Domain.Entities;

public class StudyGroup : Entity
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    /// <summary>
    /// False for one-time extra lessons (group without repeats).
    /// </summary>
    public bool IsRepeating { get; set; } = true;

    /// <summary>
    /// True: slots repeat on week 1 or 2 of the global 2-week cycle.
    /// False: slots repeat every calendar week (CycleWeek is null on lessons).
    /// </summary>
    public bool IsBiWeekly { get; set; } = true;

    public string Color { get; set; } = "#C45C3E";

    public ICollection<Lesson> Lessons { get; set; } = [];

    public ICollection<StudentStudyGroup> StudentEnrollments { get; set; } = [];
}
