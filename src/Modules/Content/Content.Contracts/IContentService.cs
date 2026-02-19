using SaasKit.SharedKernel.Api;
using SaasKit.SharedKernel.Models;

namespace Content.Contracts;

public record BlogPostDto(Guid Id, string Title, string Slug, string? Excerpt, string Status, DateTimeOffset? PublishedAt, Guid? CategoryId, string? CategoryName, int ViewCount, DateTimeOffset CreatedAt);
public record BlogCategoryDto(Guid Id, string Name, string Slug, string? Description, int DisplayOrder);
public record CreateBlogPostRequest(string Title, string Slug, string? Excerpt, string? Content, Guid? CategoryId, List<Guid>? TagIds);
public record UpdateBlogPostRequest(string? Title, string? Excerpt, string? Content, string? Status, Guid? CategoryId, List<Guid>? TagIds);

public record KBArticleDto(Guid Id, string Title, string Slug, string Language, string Status, DateTimeOffset? PublishedAt, Guid? CategoryId, int ViewCount, DateTimeOffset CreatedAt);
public record CreateKBArticleRequest(string Title, string Slug, string? Content, string Language, Guid? CategoryId);

public record PageDto(Guid Id, string Title, string Slug, string Status, DateTimeOffset? PublishedAt, DateTimeOffset CreatedAt);
public record CreatePageRequest(string Title, string Slug, string? Content, Guid? TemplateId);

public interface IBlogService
{
    Task<PagedList<BlogPostDto>> GetPostsAsync(Guid tenantId, QueryParameters qp, CancellationToken ct = default);
    Task<Result<BlogPostDto>> GetPostByIdAsync(Guid tenantId, Guid postId, CancellationToken ct = default);
    Task<Result<BlogPostDto>> CreatePostAsync(Guid tenantId, Guid authorId, CreateBlogPostRequest request, CancellationToken ct = default);
    Task<Result<BlogPostDto>> UpdatePostAsync(Guid tenantId, Guid postId, UpdateBlogPostRequest request, CancellationToken ct = default);
    Task<Result<BlogPostDto>> PublishPostAsync(Guid tenantId, Guid postId, CancellationToken ct = default);
    Task<Result<bool>> DeletePostAsync(Guid tenantId, Guid postId, CancellationToken ct = default);
    Task<PagedList<BlogCategoryDto>> GetCategoriesAsync(Guid tenantId, QueryParameters qp, CancellationToken ct = default);
}

public interface IKnowledgeBaseService
{
    Task<PagedList<KBArticleDto>> GetArticlesAsync(Guid tenantId, Guid kbId, QueryParameters qp, CancellationToken ct = default);
    Task<Result<KBArticleDto>> GetArticleByIdAsync(Guid tenantId, Guid articleId, CancellationToken ct = default);
    Task<Result<KBArticleDto>> CreateArticleAsync(Guid tenantId, Guid kbId, Guid authorId, CreateKBArticleRequest request, CancellationToken ct = default);
    Task<Result<KBArticleDto>> PublishArticleAsync(Guid tenantId, Guid articleId, CancellationToken ct = default);
}

public interface IPageService
{
    Task<PagedList<PageDto>> GetPagesAsync(Guid tenantId, QueryParameters qp, CancellationToken ct = default);
    Task<Result<PageDto>> GetPageByIdAsync(Guid tenantId, Guid pageId, CancellationToken ct = default);
    Task<Result<PageDto>> CreatePageAsync(Guid tenantId, CreatePageRequest request, CancellationToken ct = default);
    Task<Result<PageDto>> PublishPageAsync(Guid tenantId, Guid pageId, CancellationToken ct = default);
}
