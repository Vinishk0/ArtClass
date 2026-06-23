using ArtClass.Application.Caching;
using ArtClass.Application.Data;
using ArtClass.Application.Dtos;
using ArtClass.Domain.Entities;

namespace ArtClass.Application.Services;

public sealed class StudentService(UnitOfWorkExecutor db, IAppDataCache cache) : IStudentService
{
    public async Task<IReadOnlyList<StudentDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string key = "students:all";
        if (cache.TryGet(key, out IReadOnlyList<StudentDto>? cached) && cached is not null)
        {
            return cached;
        }

        var students = await db.QueryAsync(
            (unitOfWork, ct) => unitOfWork.Students.GetAllWithGroupsAsync(ct),
            cancellationToken);
        var mapped = students.Select(MapStudent).ToList();
        cache.Set(key, mapped);
        return mapped;
    }

    public async Task<StudentDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var key = $"student:{id}";
        if (cache.TryGet(key, out StudentDto? cached))
        {
            return cached;
        }

        var student = await db.QueryAsync(
            (unitOfWork, ct) => unitOfWork.Students.GetByIdWithGroupsAsync(id, ct),
            cancellationToken);
        if (student is null)
        {
            return null;
        }

        var mapped = MapStudent(student);
        cache.Set(key, mapped);
        return mapped;
    }

    public async Task<int> CreateAsync(CreateStudentRequest request, CancellationToken cancellationToken = default)
    {
        var studentId = await db.QueryAsync(async (unitOfWork, ct) =>
        {
            var student = new Student
            {
                FullName = request.FullName.Trim(),
                Phone = request.Phone?.Trim(),
                Age = request.Age,
            };

            await unitOfWork.Students.CreateAsync(student, request.GroupIds, ct);
            await unitOfWork.SaveChangesAsync(ct);
            return student.Id;
        }, cancellationToken);

        cache.InvalidateStudents();
        return studentId;
    }

    public async Task UpdateAsync(int studentId, UpdateStudentRequest request, CancellationToken cancellationToken = default)
    {
        await db.ExecuteAsync(async (unitOfWork, ct) =>
        {
            var student = await unitOfWork.Students.GetByIdForUpdateAsync(studentId, ct)
                ?? throw new InvalidOperationException("Ученик не найден");

            student.FullName = request.FullName.Trim();
            student.Phone = request.Phone?.Trim();
            student.Age = request.Age;

            await unitOfWork.Students.UpdateAsync(student, ct);
            await SyncGroupEnrollmentsAsync(unitOfWork, studentId, request.GroupIds, ct);
            await unitOfWork.SaveChangesAsync(ct);
        }, cancellationToken);

        cache.InvalidateStudents();
    }

    public async Task DeleteAsync(int studentId, CancellationToken cancellationToken = default)
    {
        await db.ExecuteAsync(async (unitOfWork, ct) =>
        {
            var student = await unitOfWork.Students.GetByIdForUpdateAsync(studentId, ct)
                ?? throw new InvalidOperationException("Ученик не найден");

            unitOfWork.Students.Remove(student);
            await unitOfWork.SaveChangesAsync(ct);
        }, cancellationToken);

        cache.InvalidateStudents();
    }

    private static async Task SyncGroupEnrollmentsAsync(
        Domain.Repositories.IUnitOfWork unitOfWork,
        int studentId,
        IReadOnlyList<int> groupIds,
        CancellationToken cancellationToken)
    {
        var student = await unitOfWork.Students.GetByIdForUpdateAsync(studentId, cancellationToken)
            ?? throw new InvalidOperationException("Ученик не найден");

        var targetIds = groupIds.Distinct().ToHashSet();
        var currentIds = student.GroupEnrollments.Select(e => e.StudyGroupId).ToHashSet();

        foreach (var groupId in currentIds.Except(targetIds))
        {
            await unitOfWork.Students.UnenrollAsync(studentId, groupId, cancellationToken);
        }

        foreach (var groupId in targetIds.Except(currentIds))
        {
            await unitOfWork.Students.EnrollAsync(studentId, groupId, cancellationToken);
        }
    }

    private static StudentDto MapStudent(Student student) =>
        new(
            student.Id,
            student.FullName,
            student.Phone,
            student.Age,
            student.GroupEnrollments.Select(e => e.StudyGroupId).ToList(),
            student.GroupEnrollments.Select(e => e.StudyGroup.Name).ToList());
}
