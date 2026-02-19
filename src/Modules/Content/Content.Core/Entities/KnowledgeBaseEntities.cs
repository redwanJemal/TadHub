using SaasKit.SharedKernel.Entities;

namespace Content.Core.Entities;

public class KnowledgeBase : TenantScopedEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsPublic { get; set; } = true;
    public string DefaultLanguage { get; set; } = "en";
    public ICollection<KBCategory> Categories { get; set; } = new List<KBCategory>();
}

public class KBCategory : TenantScopedEntity
{
    public Guid KnowledgeBaseId { get; set; }
    public KnowledgeBase KnowledgeBase { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public int DisplayOrder { get; set; }
    public Guid? ParentId { get; set; }
    public KBCategory? Parent { get; set; }
    public ICollection<KBArticle> Articles { get; set; } = new List<KBArticle>();
}

public class KBArticle : TenantScopedEntity
{
    public Guid KnowledgeBaseId { get; set; }
    public Guid? CategoryId { get; set; }
    public KBCategory? Category { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string Language { get; set; } = "en";
    public string Status { get; set; } = "draft";
    public DateTimeOffset? PublishedAt { get; set; }
    public Guid AuthorId { get; set; }
    public int ViewCount { get; set; }
    public int HelpfulCount { get; set; }
    public int NotHelpfulCount { get; set; }
    public ICollection<KBArticleVersion> Versions { get; set; } = new List<KBArticleVersion>();
}

public class KBArticleVersion : TenantScopedEntity
{
    public Guid ArticleId { get; set; }
    public KBArticle Article { get; set; } = null!;
    public int Version { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    public Guid AuthorId { get; set; }
    public string? ChangeNote { get; set; }
}
