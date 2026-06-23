using Microsoft.EntityFrameworkCore;
using ArtClass.Domain.Entities;
using ArtClass.Domain.Repositories;
using ArtClass.Infrastructure.Data;

namespace ArtClass.Infrastructure.Repositories;

internal sealed class EfStudyGroupRepository(ArtClassDbContext context)
    : EfRepository<StudyGroup>(context), IStudyGroupRepository
{
    public async Task<IReadOnlyList<StudyGroup>> GetAllWithDetailsAsync(
        CancellationToken cancellationToken = default) =>
        await Context.StudyGroups
            .AsNoTracking()
            .Include(g => g.Lessons)
            .Include(g => g.StudentEnrollments)
            .OrderBy(g => g.IsRepeating ? 0 : 1)
            .ThenBy(g => g.Name)
            .ToListAsync(cancellationToken);

    public async Task<StudyGroup?> GetByIdWithDetailsAsync(
        int id,
        CancellationToken cancellationToken = default) =>
        await Context.StudyGroups
            .AsNoTracking()
            .Include(g => g.Lessons)
            .Include(g => g.StudentEnrollments)
            .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);

    public async Task<StudyGroup?> GetByIdForUpdateAsync(
        int id,
        CancellationToken cancellationToken = default) =>
        await Context.StudyGroups
            .Include(g => g.Lessons)
            .Include(g => g.StudentEnrollments)
            .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);

    public void Remove(StudyGroup group) =>
        Context.StudyGroups.Remove(group);
}
