using Microsoft.EntityFrameworkCore;

namespace UniversityCourseEnrollment.Models.ViewModels;

public interface IPagination
{
    int PageNumber { get; }
    int PageSize { get; }
    int TotalCount { get; }
    int TotalPages { get; }
    bool HasPreviousPage { get; }
    bool HasNextPage { get; }
}

public sealed class PaginatedList<T> : IPagination
{
    public IReadOnlyList<T> Items { get; }
    public int PageNumber { get; }
    public int PageSize { get; }
    public int TotalCount { get; }
    public int TotalPages { get; }
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    private PaginatedList(List<T> items, int totalCount, int pageNumber, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
    }

    // Callers must apply a stable ordering (including a unique key) before paging.
    public static async Task<PaginatedList<T>> CreateAsync(
        IQueryable<T> query, int pageNumber = 1, int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        var totalCount = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        pageNumber = Math.Clamp(pageNumber, 1, Math.Max(1, totalPages));
        var items = await query.Skip((pageNumber - 1) * pageSize)
            .Take(pageSize).ToListAsync(cancellationToken);
        return new PaginatedList<T>(items, totalCount, pageNumber, pageSize);
    }
}
