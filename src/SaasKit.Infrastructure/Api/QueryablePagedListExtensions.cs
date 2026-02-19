using Microsoft.EntityFrameworkCore;
using SaasKit.SharedKernel.Models;

namespace SaasKit.Infrastructure.Api;

/// <summary>
/// Extension methods for creating paged lists from IQueryable.
/// </summary>
public static class QueryablePagedListExtensions
{
    /// <summary>
    /// Converts an IQueryable to a PagedList asynchronously.
    /// </summary>
    /// <typeparam name="T">Entity type.</typeparam>
    /// <param name="query">The queryable to paginate.</param>
    /// <param name="page">Page number (1-indexed).</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paged list with items and pagination metadata.</returns>
    public static async Task<PagedList<T>> ToPagedListAsync<T>(
        this IQueryable<T> query,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        // Ensure valid values
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Get items for current page
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedList<T>(items, totalCount, page, pageSize);
    }

    /// <summary>
    /// Converts an IQueryable to a PagedList asynchronously using QueryParameters.
    /// </summary>
    /// <typeparam name="T">Entity type.</typeparam>
    /// <param name="query">The queryable to paginate.</param>
    /// <param name="qp">Query parameters with page and pageSize.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paged list with items and pagination metadata.</returns>
    public static Task<PagedList<T>> ToPagedListAsync<T>(
        this IQueryable<T> query,
        SaasKit.SharedKernel.Api.QueryParameters qp,
        CancellationToken cancellationToken = default)
    {
        return query.ToPagedListAsync(qp.Page, qp.PageSize, cancellationToken);
    }
}
