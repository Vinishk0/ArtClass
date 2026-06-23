using ArtClass.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace ArtClass.Application.Data;

public sealed class UnitOfWorkExecutor(IServiceScopeFactory scopeFactory)
{
    public async Task<T> QueryAsync<T>(
        Func<IUnitOfWork, CancellationToken, Task<T>> action,
        CancellationToken cancellationToken = default)
    {
        using var scope = scopeFactory.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        return await action(unitOfWork, cancellationToken);
    }

    public async Task ExecuteAsync(
        Func<IUnitOfWork, CancellationToken, Task> action,
        CancellationToken cancellationToken = default)
    {
        using var scope = scopeFactory.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        await action(unitOfWork, cancellationToken);
    }
}
