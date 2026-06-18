using Schedule.Domain.Entities;

namespace Schedule.Domain.Repositories;

public interface IUnitOfWork
{
    ILessonRepository Lessons { get; }

    IRepository<Teacher> Teachers { get; }

    IRepository<StudyGroup> StudyGroups { get; }

    IRepository<Subject> Subjects { get; }

    IRepository<Classroom> Classrooms { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
