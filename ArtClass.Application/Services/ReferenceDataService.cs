using ArtClass.Application.Data;
using ArtClass.Application.Dtos;

namespace ArtClass.Application.Services;

public interface IReferenceDataService
{
    Task<IReadOnlyList<ReferenceItemDto>> GetTeachersAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ReferenceItemDto>> GetSubjectsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ReferenceItemDto>> GetClassroomsAsync(CancellationToken cancellationToken = default);
}

public sealed class ReferenceDataService(UnitOfWorkExecutor db) : IReferenceDataService
{
    public async Task<IReadOnlyList<ReferenceItemDto>> GetTeachersAsync(CancellationToken cancellationToken = default)
    {
        var items = await db.QueryAsync((unitOfWork, ct) => unitOfWork.Teachers.GetAllAsync(ct), cancellationToken);
        return items.Select(t => new ReferenceItemDto(t.Id, t.FullName)).ToList();
    }

    public async Task<IReadOnlyList<ReferenceItemDto>> GetSubjectsAsync(CancellationToken cancellationToken = default)
    {
        var items = await db.QueryAsync((unitOfWork, ct) => unitOfWork.Subjects.GetAllAsync(ct), cancellationToken);
        return items.Select(s => new ReferenceItemDto(s.Id, s.Name)).ToList();
    }

    public async Task<IReadOnlyList<ReferenceItemDto>> GetClassroomsAsync(CancellationToken cancellationToken = default)
    {
        var items = await db.QueryAsync((unitOfWork, ct) => unitOfWork.Classrooms.GetAllAsync(ct), cancellationToken);
        return items.Select(c => new ReferenceItemDto(c.Id, c.Name)).ToList();
    }
}
