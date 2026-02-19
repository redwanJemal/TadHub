using Meilisearch;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TadHub.Infrastructure.Settings;
using TadHub.SharedKernel.Interfaces;

namespace TadHub.Infrastructure.Search;

/// <summary>
/// Meilisearch implementation of search service.
/// Uses tenant-prefixed indexes for isolation.
/// </summary>
public sealed class MeilisearchService : ISearchService
{
    private readonly MeilisearchClient _client;
    private readonly ITenantContext _tenantContext;
    private readonly MeilisearchSettings _settings;
    private readonly ILogger<MeilisearchService> _logger;

    public MeilisearchService(
        MeilisearchClient client,
        ITenantContext tenantContext,
        IOptions<MeilisearchSettings> settings,
        ILogger<MeilisearchService> logger)
    {
        _client = client;
        _tenantContext = tenantContext;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task IndexDocumentAsync<T>(
        string indexName,
        string documentId,
        T document,
        CancellationToken cancellationToken = default) where T : class
    {
        var index = await GetOrCreateIndexAsync(indexName, cancellationToken);
        await index.AddDocumentsAsync([document], cancellationToken: cancellationToken);

        _logger.LogDebug("Indexed document {DocumentId} in index {Index}", documentId, index.Uid);
    }

    public async Task IndexDocumentsAsync<T>(
        string indexName,
        IEnumerable<T> documents,
        CancellationToken cancellationToken = default) where T : class
    {
        var index = await GetOrCreateIndexAsync(indexName, cancellationToken);
        var docList = documents.ToList();
        
        await index.AddDocumentsAsync(docList, cancellationToken: cancellationToken);

        _logger.LogDebug("Indexed {Count} documents in index {Index}", docList.Count, index.Uid);
    }

    public async Task<SearchResult<T>> SearchAsync<T>(
        string indexName,
        string query,
        SearchOptions? options = null,
        CancellationToken cancellationToken = default) where T : class
    {
        var fullIndexName = GetFullIndexName(indexName);
        var index = _client.Index(fullIndexName);

        var searchQuery = new SearchQuery
        {
            Offset = options?.Offset ?? 0,
            Limit = options?.Limit ?? 20,
            Filter = options?.Filter,
            Sort = options?.Sort,
            AttributesToRetrieve = options?.AttributesToRetrieve,
            AttributesToHighlight = options?.AttributesToHighlight
        };

        var result = await index.SearchAsync<T>(query, searchQuery, cancellationToken);

        return new SearchResult<T>
        {
            Hits = result.Hits.ToList(),
            TotalHits = result.Hits.Count,
            Offset = options?.Offset ?? 0,
            Limit = options?.Limit ?? 20,
            ProcessingTime = TimeSpan.FromMilliseconds(result.ProcessingTimeMs),
            Query = query
        };
    }

    public async Task DeleteDocumentAsync(
        string indexName,
        string documentId,
        CancellationToken cancellationToken = default)
    {
        var fullIndexName = GetFullIndexName(indexName);
        var index = _client.Index(fullIndexName);

        await index.DeleteOneDocumentAsync(documentId, cancellationToken);

        _logger.LogDebug("Deleted document {DocumentId} from index {Index}", documentId, fullIndexName);
    }

    public async Task DeleteDocumentsAsync(
        string indexName,
        IEnumerable<string> documentIds,
        CancellationToken cancellationToken = default)
    {
        var fullIndexName = GetFullIndexName(indexName);
        var index = _client.Index(fullIndexName);
        var ids = documentIds.ToList();

        await index.DeleteDocumentsAsync(ids, cancellationToken);

        _logger.LogDebug("Deleted {Count} documents from index {Index}", ids.Count, fullIndexName);
    }

    public async Task ConfigureIndexAsync(
        string indexName,
        IndexConfiguration config,
        CancellationToken cancellationToken = default)
    {
        var index = await GetOrCreateIndexAsync(indexName, cancellationToken);

        var settings = new Meilisearch.Settings
        {
            SearchableAttributes = config.SearchableAttributes,
            FilterableAttributes = config.FilterableAttributes,
            SortableAttributes = config.SortableAttributes,
            DisplayedAttributes = config.DisplayedAttributes
        };

        await index.UpdateSettingsAsync(settings, cancellationToken);

        _logger.LogInformation("Configured index {Index}", index.Uid);
    }

    private async Task<Meilisearch.Index> GetOrCreateIndexAsync(
        string indexName,
        CancellationToken cancellationToken)
    {
        var fullIndexName = GetFullIndexName(indexName);

        try
        {
            return await _client.GetIndexAsync(fullIndexName, cancellationToken);
        }
        catch (MeilisearchApiError ex) when (ex.Code == "index_not_found")
        {
            var task = await _client.CreateIndexAsync(fullIndexName, "id", cancellationToken);
            await _client.WaitForTaskAsync(task.TaskUid, cancellationToken: cancellationToken);
            return await _client.GetIndexAsync(fullIndexName, cancellationToken);
        }
    }

    private string GetFullIndexName(string indexName)
    {
        if (!_tenantContext.IsResolved)
            return $"{_settings.IndexPrefix}global_{indexName}";

        return $"{_settings.IndexPrefix}{_tenantContext.TenantId}_{indexName}";
    }
}
