using ArtClass.Domain.Common;

namespace ArtClass.Domain.Entities;

public class StudyGroup : Entity
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public ICollection<Lesson> Lessons { get; set; } = [];
}
