using Shared.DataAccess.BuildingBlocks.Repositories;
using Shared.Services.BuildingBlocks.Common;

namespace Shared.Services.BuildingBlocks.Repositories;

public interface IUnitOfWork : IDisposable
{
    IRepository<TEntity> Repository<TEntity>()
        where TEntity : Entity;

    Task<int> CommitAsync(CancellationToken cancellationToken = default);

    Task ExecuteInTransactionAsync(Func<Task> action);
}