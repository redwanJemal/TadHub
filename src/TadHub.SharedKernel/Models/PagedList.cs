namespace TadHub.SharedKernel.Models;

/// <summary>
/// Represents a paginated list of items with metadata.
/// </summary>
/// <typeparam name="T">The type of items in the list.</typeparam>
public sealed class PagedList<T>
{
    /// <summary>
    /// The items in the current page.
    /// </summary>
    public IReadOnlyList<T> Items { get; }

    /// <summary>
    /// Total number of items across all pages.
    /// </summary>
    public int TotalCount { get; }

    /// <summary>
    /// Current page number (1-indexed).
    /// </summary>
    public int Page { get; }

    /// <summary>
    /// Number of items per page.
    /// </summary>
    public int PageSize { get; }

    /// <summary>
    /// Total number of pages.
    /// </summary>
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    /// <summary>
    /// Indicates if there's a next page.
    /// </summary>
    public bool HasNextPage => Page < TotalPages;

    /// <summary>
    /// Indicates if there's a previous page.
    /// </summary>
    public bool HasPreviousPage => Page > 1;

    public PagedList(IReadOnlyList<T> items, int totalCount, int page, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        Page = page;
        PageSize = pageSize;
    }

    /// <summary>
    /// Creates an empty paged list.
    /// </summary>
    public static PagedList<T> Empty(int page = 1, int pageSize = 20) =>
        new([], 0, page, pageSize);

    /// <summary>
    /// Maps the items to a new type.
    /// </summary>
    public PagedList<TNew> Map<TNew>(Func<T, TNew> mapper)
    {
        var mappedItems = Items.Select(mapper).ToList();
        return new PagedList<TNew>(mappedItems, TotalCount, Page, PageSize);
    }
}
