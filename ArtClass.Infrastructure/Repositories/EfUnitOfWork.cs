using ArtClass.Domain.Entities;
using ArtClass.Domain.Repositories;
using ArtClass.Infrastructure.Data;

namespace ArtClass.Infrastructure.Repositories;

internal sealed class EfUnitOfWork(ArtClassDbContext context) : IUnitOfWork
{
    private ILessonRepository? _lessons;
    private IStudentRepository? _students;
    private IScheduleSettingsRepository? _scheduleSettings;
    private IRepository<Teacher>? _teachers;
    private IStudyGroupRepository? _studyGroups;
    private IRepository<Subject>? _subjects;
    private IRepository<Classroom>? _classrooms;

    public ILessonRepository Lessons => _lessons ??= new EfLessonRepository(context);

    public IStudentRepository Students => _students ??= new EfStudentRepository(context);

    public IScheduleSettingsRepository ScheduleSettings => _scheduleSettings ??= new EfScheduleSettingsRepository(context);

    public IRepository<Teacher> Teachers => _teachers ??= new EfRepository<Teacher>(context);

    public IStudyGroupRepository StudyGroups => _studyGroups ??= new EfStudyGroupRepository(context);

    public IRepository<Subject> Subjects => _subjects ??= new EfRepository<Subject>(context);

    public IRepository<Classroom> Classrooms => _classrooms ??= new EfRepository<Classroom>(context);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        context.SaveChangesAsync(cancellationToken);
}
