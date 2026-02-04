using Shared.Services.BuildingBlocks.Common;

namespace Shared.Services.BuildingBlocks.Repositories;

public interface IWriteRepository<TEntity> where TEntity : Entity
{
    Task<TEntity> AddEntityAsync(TEntity entity);
    Task<TEntity> ModifyEntityAsync(TEntity entity);
    Task<TEntity> DeleteEntityAsync(TEntity entity);
}