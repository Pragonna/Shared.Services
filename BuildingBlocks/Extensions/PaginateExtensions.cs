using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Shared.Services.BuildingBlocks.Extensions;

public static class PaginateExtensions
{
    public static Paginate<TDto> MapTo<TEntity, TDto>(
        this Paginate<TEntity> source,
        Func<TEntity, TDto> map)
    {
        return new Paginate<TDto>(
            source.Items.Select(map).ToList(),
            source.PageIndex,
            source.PageSize,
            source.TotalCount
        );
    }

    public static Paginate<TDto> MapTo<TSource, TDto>(
        this Paginate<TSource> source,
        IMapper mapper)
    {
        return new Paginate<TDto>(
            mapper.Map<List<TDto>>(source.Items),
            source.PageIndex,
            source.PageSize,
            source.TotalCount
        );
    }

    public static async Task<Paginate<TEntity>> ToPaginateAsync<TEntity>(
        this IQueryable<TEntity> query, int pageIndex = 1, int pageSize = 10)
    {
        pageIndex = pageIndex < 1 ? 1 : pageIndex;
        pageSize  = pageSize  < 1 ? 10 : pageSize;
        
        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new Paginate<TEntity>(items, pageIndex, pageSize, totalCount);
    }
}