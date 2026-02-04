using AutoMapper;

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
}