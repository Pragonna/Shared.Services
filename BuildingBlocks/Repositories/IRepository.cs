using Shared.Services.BuildingBlocks.Common;
using Shared.Services.BuildingBlocks.Repositories;

namespace Shared.DataAccess.BuildingBlocks.Repositories;

public interface IRepository<TEntity> :
    IReadRepository<TEntity>,
    IWriteRepository<TEntity>
    where TEntity : Entity
{
}