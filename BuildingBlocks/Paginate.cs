namespace Shared.Services.BuildingBlocks;

public class Paginate<TEntity>
{
    public List<TEntity> Items { get; set; } = [];
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPrevious => PageIndex > 1;
    public bool HasNext => PageIndex < TotalPages;

    public Paginate(IEnumerable<TEntity> items, int pageIndex, int pageSize, int totalCount)
    {
        Items = items.ToList();
        PageIndex = pageIndex;
        PageSize = pageSize;
        TotalCount = totalCount;
    }
}