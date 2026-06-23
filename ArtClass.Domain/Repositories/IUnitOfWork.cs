using ArtClass.Domain.Entities;

namespace ArtClass.Domain.Repositories;

public interface IUnitOfWork
{
    ILessonRepository Lessons { get; }

    IStudentRepository Students { get; }

    IScheduleSettingsRepository ScheduleSettings { get; }

    IRepository<Teacher> Teachers { get; }

    IStudyGroupRepository StudyGroups { get; }

    IRepository<Subject> Subjects { get; }

    IRepository<Classroom> Classrooms { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
