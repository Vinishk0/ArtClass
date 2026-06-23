namespace ArtClass.Domain.Entities;

public class StudentStudyGroup
{
    public int StudentId { get; set; }

    public Student Student { get; set; } = null!;

    public int StudyGroupId { get; set; }

    public StudyGroup StudyGroup { get; set; } = null!;
}
