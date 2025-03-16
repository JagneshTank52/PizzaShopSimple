using Microsoft.EntityFrameworkCore;

namespace PizzaShop.Service.Helper;

public class PaginatedList<T> : List<T>
{
     public int PageIndex { get; private set; }
    public int TotalPages { get; private set; }
    public int PageSize { get; private set; }
    public int TotalCount { get; private set; }

    public int FromCount => (PageIndex - 1) * PageSize;
    public int ToCount => FromCount + PageSize;

    public bool HasPreviousPage => PageIndex > 1;

    public bool HasNextPage => PageIndex < TotalPages;

    public PaginatedList(List<T> items, int totalCount, int pageIndex, int pageSize)
    {
        PageIndex = pageIndex;
        PageSize = pageSize;
        TotalCount = totalCount;
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        this.AddRange(items);
    }

    public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageIndex, int pageSize)
    {
        var totalCount = await source.CountAsync();
        var items = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
        return new PaginatedList<T>(items, totalCount, pageIndex, pageSize);
    }
}
