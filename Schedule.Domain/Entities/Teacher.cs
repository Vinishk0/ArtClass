using Schedule.Domain.Common;

namespace Schedule.Domain.Entities;

public class Teacher : Entity
{
    public string FullName { get; set; } = string.Empty;

    public string? Specialization { get; set; }

    public ICollection<Lesson> Lessons { get; set; } = [];
}
