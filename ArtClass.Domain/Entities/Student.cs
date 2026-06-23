using ArtClass.Domain.Common;

namespace ArtClass.Domain.Entities;

public class Student : Entity
{
    public string FullName { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public int? Age { get; set; }

    public ICollection<StudentStudyGroup> GroupEnrollments { get; set; } = [];
}
