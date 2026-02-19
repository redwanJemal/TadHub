using System.Linq.Expressions;
using Content.Contracts;
using Content.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SaasKit.Infrastructure.Api;
using SaasKit.Infrastructure.Persistence;
using SaasKit.SharedKernel.Api;
using SaasKit.SharedKernel.Interfaces;
using SaasKit.SharedKernel.Models;

namespace Content.Core.Services;

public class BlogService : IBlogService
{
    private readonly AppDbContext _db;
    private readonly IClock _clock;
    private readonly ILogger<BlogService> _logger;

    private static readonly Dictionary<string, Expression<Func<BlogPost, object>>> PostFilters = new()
    {
        ["status"] = x => x.Status,
        ["categoryId"] = x => x.CategoryId!,
        ["publishedAt"] = x => x.PublishedAt!
    };

    public BlogService(AppDbContext db, IClock clock, ILogger<BlogService> logger)
    {
        _db = db;
        _clock = clock;
        _logger = logger;
    }

    public async Task<PagedList<BlogPostDto>> GetPostsAsync(Guid tenantId, QueryParameters qp, CancellationToken ct = default)
    {
        var query = _db.Set<BlogPost>().AsNoTracking().Where(x => x.TenantId == tenantId)
            .Include(x => x.Category)
            .ApplyFilters(qp.Filters, PostFilters)
            .ApplySort(qp.GetSortFields(), new Dictionary<string, Expression<Func<BlogPost, object>>> { ["createdAt"] = x => x.CreatedAt, ["publishedAt"] = x => x.PublishedAt!, ["title"] = x => x.Title });

        return await query.Select(x => new BlogPostDto(x.Id, x.Title, x.Slug, x.Excerpt, x.Status, x.PublishedAt, x.CategoryId, x.Category != null ? x.Category.Name : null, x.ViewCount, x.CreatedAt)).ToPagedListAsync(qp, ct);
    }

    public async Task<Result<BlogPostDto>> GetPostByIdAsync(Guid tenantId, Guid postId, CancellationToken ct = default)
    {
        var post = await _db.Set<BlogPost>().AsNoTracking().Include(x => x.Category).FirstOrDefaultAsync(x => x.Id == postId && x.TenantId == tenantId, ct);
        if (post is null) return Result<BlogPostDto>.NotFound("Post not found");
        return Result<BlogPostDto>.Success(new BlogPostDto(post.Id, post.Title, post.Slug, post.Excerpt, post.Status, post.PublishedAt, post.CategoryId, post.Category?.Name, post.ViewCount, post.CreatedAt));
    }

    public async Task<Result<BlogPostDto>> CreatePostAsync(Guid tenantId, Guid authorId, CreateBlogPostRequest request, CancellationToken ct = default)
    {
        if (await _db.Set<BlogPost>().AnyAsync(x => x.TenantId == tenantId && x.Slug == request.Slug, ct))
            return Result<BlogPostDto>.Conflict($"Post with slug '{request.Slug}' already exists");

        var post = new BlogPost { Id = Guid.NewGuid(), TenantId = tenantId, Title = request.Title, Slug = request.Slug, Excerpt = request.Excerpt, Content = request.Content, CategoryId = request.CategoryId, AuthorId = authorId, Status = "draft" };
        _db.Set<BlogPost>().Add(post);
        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Created blog post {PostId} in tenant {TenantId}", post.Id, tenantId);
        return Result<BlogPostDto>.Success(new BlogPostDto(post.Id, post.Title, post.Slug, post.Excerpt, post.Status, post.PublishedAt, post.CategoryId, null, 0, post.CreatedAt));
    }

    public async Task<Result<BlogPostDto>> UpdatePostAsync(Guid tenantId, Guid postId, UpdateBlogPostRequest request, CancellationToken ct = default)
    {
        var post = await _db.Set<BlogPost>().Include(x => x.Category).FirstOrDefaultAsync(x => x.Id == postId && x.TenantId == tenantId, ct);
        if (post is null) return Result<BlogPostDto>.NotFound("Post not found");
        if (request.Title is not null) post.Title = request.Title;
        if (request.Excerpt is not null) post.Excerpt = request.Excerpt;
        if (request.Content is not null) post.Content = request.Content;
        if (request.Status is not null) post.Status = request.Status;
        if (request.CategoryId.HasValue) post.CategoryId = request.CategoryId;
        await _db.SaveChangesAsync(ct);
        return Result<BlogPostDto>.Success(new BlogPostDto(post.Id, post.Title, post.Slug, post.Excerpt, post.Status, post.PublishedAt, post.CategoryId, post.Category?.Name, post.ViewCount, post.CreatedAt));
    }

    public async Task<Result<BlogPostDto>> PublishPostAsync(Guid tenantId, Guid postId, CancellationToken ct = default)
    {
        var post = await _db.Set<BlogPost>().Include(x => x.Category).FirstOrDefaultAsync(x => x.Id == postId && x.TenantId == tenantId, ct);
        if (post is null) return Result<BlogPostDto>.NotFound("Post not found");
        post.Status = "published";
        post.PublishedAt = _clock.UtcNow;
        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Published blog post {PostId}", postId);
        return Result<BlogPostDto>.Success(new BlogPostDto(post.Id, post.Title, post.Slug, post.Excerpt, post.Status, post.PublishedAt, post.CategoryId, post.Category?.Name, post.ViewCount, post.CreatedAt));
    }

    public async Task<Result<bool>> DeletePostAsync(Guid tenantId, Guid postId, CancellationToken ct = default)
    {
        var post = await _db.Set<BlogPost>().FirstOrDefaultAsync(x => x.Id == postId && x.TenantId == tenantId, ct);
        if (post is null) return Result<bool>.NotFound("Post not found");
        _db.Set<BlogPost>().Remove(post);
        await _db.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }

    public async Task<PagedList<BlogCategoryDto>> GetCategoriesAsync(Guid tenantId, QueryParameters qp, CancellationToken ct = default)
    {
        var query = _db.Set<BlogCategory>().AsNoTracking().Where(x => x.TenantId == tenantId).OrderBy(x => x.DisplayOrder);
        return await query.Select(x => new BlogCategoryDto(x.Id, x.Name, x.Slug, x.Description, x.DisplayOrder)).ToPagedListAsync(qp, ct);
    }
}
