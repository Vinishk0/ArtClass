using ArtClass.Domain.Entities;

namespace ArtClass.Domain.Repositories;

public interface IStudyGroupRepository : IRepository<StudyGroup>
{
    Task<IReadOnlyList<StudyGroup>> GetAllWithDetailsAsync(CancellationToken cancellationToken = default);

    Task<StudyGroup?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default);

    Task<StudyGroup?> GetByIdForUpdateAsync(int id, CancellationToken cancellationToken = default);

    void Remove(StudyGroup group);
}
