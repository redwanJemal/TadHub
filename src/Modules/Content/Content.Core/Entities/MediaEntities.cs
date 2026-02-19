using SaasKit.SharedKernel.Entities;

namespace Content.Core.Entities;

public class Media : TenantScopedEntity
{
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public long Size { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public Guid? FolderId { get; set; }
    public MediaFolder? Folder { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public string? Alt { get; set; }
    public string? Caption { get; set; }
    public Guid UploadedByUserId { get; set; }
}

public class MediaFolder : TenantScopedEntity
{
    public string Name { get; set; } = string.Empty;
    public Guid? ParentId { get; set; }
    public MediaFolder? Parent { get; set; }
    public ICollection<Media> Files { get; set; } = new List<Media>();
    public ICollection<MediaFolder> Children { get; set; } = new List<MediaFolder>();
}

public class ContentTranslation : TenantScopedEntity
{
    public string EntityType { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public string Field { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public class ContentRevision : TenantScopedEntity
{
    public string EntityType { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public int Version { get; set; }
    public string? Data { get; set; } // JSONB
    public Guid AuthorId { get; set; }
    public string? Note { get; set; }
}
