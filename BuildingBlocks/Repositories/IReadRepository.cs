using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using Shared.Services.BuildingBlocks.Common;

namespace Shared.Services.BuildingBlocks.Repositories;

public interface IReadRepository<TEntity> where TEntity : Entity
{
    Task<Paginate<TEntity>> GetPaginateAsync(
        Expression<Func<TEntity, bool>> filter = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
        int pageIndex = 1,
        int pageSize = 10,
        bool enableTracking = true);

    Task<List<TEntity>> GetAsync(
        Expression<Func<TEntity, bool>> filter = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
        int pageIndex = 1,
        int pageSize = 10,
        bool enableTracking = true);

    Task<TEntity> GetByIdAsync(Guid id,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
        bool enableTracking = true);

    Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> filter);
}