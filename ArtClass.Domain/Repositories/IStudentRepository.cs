using ArtClass.Domain.Entities;

namespace ArtClass.Domain.Repositories;

public interface IStudentRepository : IRepository<Student>
{
    Task<IReadOnlyList<Student>> GetAllWithGroupsAsync(CancellationToken cancellationToken = default);

    Task<Student?> GetByIdWithGroupsAsync(int id, CancellationToken cancellationToken = default);

    Task<Student?> GetByIdForUpdateAsync(int id, CancellationToken cancellationToken = default);

    Task EnrollAsync(int studentId, int studyGroupId, CancellationToken cancellationToken = default);

    Task UnenrollAsync(int studentId, int studyGroupId, CancellationToken cancellationToken = default);

    Task CreateAsync(Student student, IReadOnlyList<int> groupIds, CancellationToken cancellationToken = default);

    void Remove(Student student);
}
