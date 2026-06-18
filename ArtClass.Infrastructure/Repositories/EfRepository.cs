using Microsoft.EntityFrameworkCore;
using ArtClass.Domain.Common;
using ArtClass.Domain.Repositories;
using ArtClass.Infrastructure.Data;

namespace ArtClass.Infrastructure.Repositories;

internal class EfRepository<T>(ArtClassDbContext context) : IRepository<T>
    where T : Entity
{
    protected ArtClassDbContext Context => context;

    protected DbSet<T> DbSet => context.Set<T>();

    public async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        await DbSet.FindAsync([id], cancellationToken);

    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await DbSet.AsNoTracking().ToListAsync(cancellationToken);

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await DbSet.AddAsync(entity, cancellationToken);
    }

    public Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        DbSet.Update(entity);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity is not null)
        {
            DbSet.Remove(entity);
        }
    }
}
