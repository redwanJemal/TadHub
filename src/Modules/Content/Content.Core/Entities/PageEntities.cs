using TadHub.SharedKernel.Entities;

namespace Content.Core.Entities;

public class Page : TenantScopedEntity
{
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string Status { get; set; } = "draft";
    public DateTimeOffset? PublishedAt { get; set; }
    public string? SeoTitle { get; set; }
    public string? SeoDescription { get; set; }
    public Guid? TemplateId { get; set; }
    public Template? Template { get; set; }
    public ICollection<PageBlock> Blocks { get; set; } = new List<PageBlock>();
    public ICollection<PageVersion> Versions { get; set; } = new List<PageVersion>();
}

public class PageBlock : TenantScopedEntity
{
    public Guid PageId { get; set; }
    public Page Page { get; set; } = null!;
    public string Type { get; set; } = string.Empty; // text, image, video, etc
    public string? Content { get; set; } // JSONB
    public int DisplayOrder { get; set; }
}

public class PageVersion : TenantScopedEntity
{
    public Guid PageId { get; set; }
    public Page Page { get; set; } = null!;
    public int Version { get; set; }
    public string? Content { get; set; }
    public Guid AuthorId { get; set; }
}

public class Template : TenantScopedEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Schema { get; set; } // JSONB - block schema
    public string? DefaultContent { get; set; }
    public bool IsActive { get; set; } = true;
}
