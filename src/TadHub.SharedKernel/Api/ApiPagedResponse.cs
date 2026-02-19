using TadHub.SharedKernel.Models;

namespace TadHub.SharedKernel.Api;

/// <summary>
/// Standard API response envelope for paginated collections.
/// </summary>
/// <typeparam name="T">The type of items in the collection.</typeparam>
public sealed class ApiPagedResponse<T>
{
    /// <summary>
    /// The collection of items.
    /// </summary>
    public IReadOnlyList<T> Data { get; init; } = [];

    /// <summary>
    /// Response metadata.
    /// </summary>
    public ApiMeta Meta { get; init; } = new();

    /// <summary>
    /// Pagination metadata.
    /// </summary>
    public PaginationMeta Pagination { get; init; } = new();

    /// <summary>
    /// Creates a paged response from a PagedList.
    /// </summary>
    public static ApiPagedResponse<T> From(PagedList<T> pagedList, string? requestId = null) => new()
    {
        Data = pagedList.Items,
        Meta = ApiMeta.Create(requestId),
        Pagination = PaginationMeta.From(pagedList)
    };
}

/// <summary>
/// Pagination metadata for collection responses.
/// </summary>
public sealed class PaginationMeta
{
    /// <summary>
    /// Current page number (1-indexed).
    /// </summary>
    public int Page { get; init; }

    /// <summary>
    /// Number of items per page.
    /// </summary>
    public int PageSize { get; init; }

    /// <summary>
    /// Total number of items across all pages.
    /// </summary>
    public int TotalCount { get; init; }

    /// <summary>
    /// Total number of pages.
    /// </summary>
    public int TotalPages { get; init; }

    /// <summary>
    /// Indicates if there's a next page.
    /// </summary>
    public bool HasNextPage { get; init; }

    /// <summary>
    /// Indicates if there's a previous page.
    /// </summary>
    public bool HasPreviousPage { get; init; }

    /// <summary>
    /// Creates pagination metadata from a PagedList.
    /// </summary>
    public static PaginationMeta From<T>(PagedList<T> pagedList) => new()
    {
        Page = pagedList.Page,
        PageSize = pagedList.PageSize,
        TotalCount = pagedList.TotalCount,
        TotalPages = pagedList.TotalPages,
        HasNextPage = pagedList.HasNextPage,
        HasPreviousPage = pagedList.HasPreviousPage
    };
}
