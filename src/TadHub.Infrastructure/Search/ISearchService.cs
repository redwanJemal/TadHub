namespace TadHub.Infrastructure.Search;

/// <summary>
/// Search service interface for full-text search operations.
/// Uses tenant-prefixed indexes for isolation.
/// </summary>
public interface ISearchService
{
    /// <summary>
    /// Indexes a document for searching.
    /// </summary>
    /// <typeparam name="T">The document type.</typeparam>
    /// <param name="indexName">The index name (will be prefixed with tenant).</param>
    /// <param name="documentId">The document's unique identifier.</param>
    /// <param name="document">The document to index.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task IndexDocumentAsync<T>(
        string indexName,
        string documentId,
        T document,
        CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Indexes multiple documents.
    /// </summary>
    /// <typeparam name="T">The document type.</typeparam>
    /// <param name="indexName">The index name.</param>
    /// <param name="documents">The documents to index.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task IndexDocumentsAsync<T>(
        string indexName,
        IEnumerable<T> documents,
        CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Searches for documents.
    /// </summary>
    /// <typeparam name="T">The document type.</typeparam>
    /// <param name="indexName">The index name.</param>
    /// <param name="query">The search query.</param>
    /// <param name="options">Search options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Search results.</returns>
    Task<SearchResult<T>> SearchAsync<T>(
        string indexName,
        string query,
        SearchOptions? options = null,
        CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Deletes a document from the index.
    /// </summary>
    /// <param name="indexName">The index name.</param>
    /// <param name="documentId">The document ID to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task DeleteDocumentAsync(
        string indexName,
        string documentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes multiple documents from the index.
    /// </summary>
    /// <param name="indexName">The index name.</param>
    /// <param name="documentIds">The document IDs to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task DeleteDocumentsAsync(
        string indexName,
        IEnumerable<string> documentIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Configures index settings (searchable attributes, filterable attributes, etc.).
    /// </summary>
    /// <param name="indexName">The index name.</param>
    /// <param name="config">The index configuration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task ConfigureIndexAsync(
        string indexName,
        IndexConfiguration config,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Search options for querying.
/// </summary>
public sealed record SearchOptions
{
    public int Offset { get; init; } = 0;
    public int Limit { get; init; } = 20;
    public string[]? Filter { get; init; }
    public string[]? Sort { get; init; }
    public string[]? AttributesToRetrieve { get; init; }
    public string[]? AttributesToHighlight { get; init; }
}

/// <summary>
/// Search result container.
/// </summary>
public sealed record SearchResult<T>
{
    public IReadOnlyList<T> Hits { get; init; } = [];
    public int TotalHits { get; init; }
    public int Offset { get; init; }
    public int Limit { get; init; }
    public TimeSpan ProcessingTime { get; init; }
    public string Query { get; init; } = string.Empty;
}

/// <summary>
/// Index configuration for search.
/// </summary>
public sealed record IndexConfiguration
{
    public string[]? SearchableAttributes { get; init; }
    public string[]? FilterableAttributes { get; init; }
    public string[]? SortableAttributes { get; init; }
    public string[]? DisplayedAttributes { get; init; }
    public string? PrimaryKey { get; init; }
}
