using Shared.Services.BuildingBlocks.Common;

namespace Shared.Services.BuildingBlocks.Repositories;

public interface IWriteRepository<TEntity> where TEntity : Entity
{
    Task<TEntity> AddEntity(TEntity entity);
    Task<TEntity> ModifyEntity(TEntity entity);
    Task<TEntity> DeleteEntity(TEntity entity);
}