using Schedule.Domain.Common;

namespace Schedule.Domain.Entities;

public class Subject : Entity
{
    public string Name { get; set; } = string.Empty;

    public ICollection<Lesson> Lessons { get; set; } = [];
}
