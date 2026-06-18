using ArtClass.Domain.Common;

namespace ArtClass.Domain.Entities;

public class Teacher : Entity
{
    public string FullName { get; set; } = string.Empty;

    public string? Specialization { get; set; }

    public ICollection<Lesson> Lessons { get; set; } = [];
}
