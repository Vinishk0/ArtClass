namespace ArtClass.Application.Services;

public interface IGroupService
{
    Task<IReadOnlyList<Dtos.GroupDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Dtos.GroupDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<int> CreateRepeatingGroupAsync(Dtos.CreateGroupRequest request, CancellationToken cancellationToken = default);

    Task<int> CreateExtraLessonAsync(Dtos.CreateExtraLessonRequest request, CancellationToken cancellationToken = default);

    Task UpdateRepeatingGroupAsync(int groupId, Dtos.UpdateGroupRequest request, CancellationToken cancellationToken = default);

    Task UpdateExtraLessonAsync(int groupId, Dtos.UpdateExtraLessonRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(int groupId, CancellationToken cancellationToken = default);
}
