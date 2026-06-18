using Schedule.Domain.Entities;
using Schedule.Domain.Repositories;
using Schedule.Infrastructure.Data;

namespace Schedule.Infrastructure.Repositories;

internal sealed class EfUnitOfWork(ScheduleDbContext context) : IUnitOfWork
{
    private ILessonRepository? _lessons;
    private IRepository<Teacher>? _teachers;
    private IRepository<StudyGroup>? _studyGroups;
    private IRepository<Subject>? _subjects;
    private IRepository<Classroom>? _classrooms;

    public ILessonRepository Lessons => _lessons ??= new EfLessonRepository(context);

    public IRepository<Teacher> Teachers => _teachers ??= new EfRepository<Teacher>(context);

    public IRepository<StudyGroup> StudyGroups => _studyGroups ??= new EfRepository<StudyGroup>(context);

    public IRepository<Subject> Subjects => _subjects ??= new EfRepository<Subject>(context);

    public IRepository<Classroom> Classrooms => _classrooms ??= new EfRepository<Classroom>(context);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        context.SaveChangesAsync(cancellationToken);
}
