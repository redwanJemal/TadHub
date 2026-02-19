using SaasKit.SharedKernel.Entities;

namespace Content.Core.Entities;

public class BlogPost : TenantScopedEntity
{
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Excerpt { get; set; }
    public string? Content { get; set; }
    public string Status { get; set; } = "draft"; // draft, published, archived
    public DateTimeOffset? PublishedAt { get; set; }
    public Guid? CategoryId { get; set; }
    public BlogCategory? Category { get; set; }
    public Guid AuthorId { get; set; }
    public string? FeaturedImageUrl { get; set; }
    public string? SeoTitle { get; set; }
    public string? SeoDescription { get; set; }
    public int ViewCount { get; set; }
    public ICollection<BlogPostTag> Tags { get; set; } = new List<BlogPostTag>();
}

public class BlogCategory : TenantScopedEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public ICollection<BlogPost> Posts { get; set; } = new List<BlogPost>();
}

public class BlogTag : TenantScopedEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public ICollection<BlogPostTag> Posts { get; set; } = new List<BlogPostTag>();
}

public class BlogPostTag : TenantScopedEntity
{
    public Guid BlogPostId { get; set; }
    public BlogPost BlogPost { get; set; } = null!;
    public Guid BlogTagId { get; set; }
    public BlogTag BlogTag { get; set; } = null!;
}
