using Shared.Services.BuildingBlocks.Common;

namespace Shared.Services.BuildingBlocks.Repositories;

public interface IWriteRepository<TEntity> where TEntity : Entity
{
    TEntity AddEntity(TEntity entity);
    TEntity ModifyEntity(TEntity entity);
    TEntity DeleteEntity(TEntity entity);
}