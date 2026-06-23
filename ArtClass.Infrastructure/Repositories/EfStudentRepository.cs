using Microsoft.EntityFrameworkCore;
using ArtClass.Domain.Entities;
using ArtClass.Domain.Repositories;
using ArtClass.Infrastructure.Data;

namespace ArtClass.Infrastructure.Repositories;

internal sealed class EfStudentRepository(ArtClassDbContext context)
    : EfRepository<Student>(context), IStudentRepository
{
    public async Task<IReadOnlyList<Student>> GetAllWithGroupsAsync(
        CancellationToken cancellationToken = default) =>
        await Context.Students
            .AsNoTracking()
            .Include(s => s.GroupEnrollments)
            .ThenInclude(e => e.StudyGroup)
            .OrderBy(s => s.FullName)
            .ToListAsync(cancellationToken);

    public async Task<Student?> GetByIdWithGroupsAsync(
        int id,
        CancellationToken cancellationToken = default) =>
        await Context.Students
            .AsNoTracking()
            .Include(s => s.GroupEnrollments)
            .ThenInclude(e => e.StudyGroup)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public async Task<Student?> GetByIdForUpdateAsync(
        int id,
        CancellationToken cancellationToken = default) =>
        await Context.Students
            .Include(s => s.GroupEnrollments)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public async Task EnrollAsync(
        int studentId,
        int studyGroupId,
        CancellationToken cancellationToken = default)
    {
        var exists = await Context.StudentStudyGroups
            .AnyAsync(e => e.StudentId == studentId && e.StudyGroupId == studyGroupId, cancellationToken);

        if (exists)
        {
            return;
        }

        await Context.StudentStudyGroups.AddAsync(
            new StudentStudyGroup { StudentId = studentId, StudyGroupId = studyGroupId },
            cancellationToken);
    }

    public async Task UnenrollAsync(
        int studentId,
        int studyGroupId,
        CancellationToken cancellationToken = default)
    {
        var enrollment = await Context.StudentStudyGroups
            .FirstOrDefaultAsync(
                e => e.StudentId == studentId && e.StudyGroupId == studyGroupId,
                cancellationToken);

        if (enrollment is not null)
        {
            Context.StudentStudyGroups.Remove(enrollment);
        }
    }

    public async Task CreateAsync(
        Student student,
        IReadOnlyList<int> groupIds,
        CancellationToken cancellationToken = default)
    {
        await Context.Students.AddAsync(student, cancellationToken);
        await Context.SaveChangesAsync(cancellationToken);

        foreach (var groupId in groupIds.Distinct())
        {
            await EnrollAsync(student.Id, groupId, cancellationToken);
        }
    }

    public void Remove(Student student) =>
        Context.Students.Remove(student);
}
