using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Shared.Services.BuildingBlocks;
using Shared.Services.BuildingBlocks.Common;

namespace Shared.DataAccess.BuildingBlocks.Repositories;

public abstract class BaseRepository<TEntity, TContext>(TContext context) : IRepository<TEntity>
    where TEntity : Entity
    where TContext : DbContext
{
    private IQueryable<TEntity> _query => context.Set<TEntity>();

    public virtual async Task<Paginate<TEntity>> GetPaginateAsync(Expression<Func<TEntity, bool>> filter = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null, int pageIndex = 1,
        int pageSize = 10,
        bool enableTracking = true)
    {
        IQueryable<TEntity> query = _query;
        if (filter != null) query = query.Where(filter);
        if (include != null) query = include(query);
        if (!enableTracking) query = query.AsNoTracking();

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new Paginate<TEntity>(items, pageIndex, pageSize, totalCount);
    }

    public virtual async Task<IQueryable<TEntity>> GetAsync(Expression<Func<TEntity, bool>> filter,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null, int pageIndex = 1,
        int pageSize = 10,
        bool enableTracking = true)
    {
        IQueryable<TEntity> query = _query;
        if (filter != null) query = query.Where(filter);
        if (include != null) query = include(query);
        if (!enableTracking) query = query.AsNoTracking();

        return query;
    }

    public virtual async Task<TEntity> GetByIdAsync(Guid id,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
        bool enableTracking = true)
    {
        IQueryable<TEntity> query = _query;
        if (include != null) query = include(query);
        if (!enableTracking) query = query.AsNoTracking();

        return await query.FirstOrDefaultAsync(e => e.Id == id);
    }

    public virtual async Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> filter)
    {
        return await _query.FirstOrDefaultAsync(filter);
    }

    public virtual async Task<TEntity> AddEntityAsync(TEntity entity)
    {
        context.Entry(entity).State = EntityState.Added;
        return entity;
    }

    public virtual async Task<TEntity> ModifyEntityAsync(TEntity entity)
    {
        context.Entry(entity).State = EntityState.Modified;
        return entity;
    }

    public virtual async Task<TEntity> DeleteEntityAsync(TEntity entity)
    {
        context.Entry(entity).State = EntityState.Deleted;
        return entity;
    }
}