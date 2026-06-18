using Microsoft.EntityFrameworkCore;
using Schedule.Domain.Common;
using Schedule.Domain.Repositories;
using Schedule.Infrastructure.Data;

namespace Schedule.Infrastructure.Repositories;

internal class EfRepository<T>(ScheduleDbContext context) : IRepository<T>
    where T : Entity
{
    protected ScheduleDbContext Context => context;

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
