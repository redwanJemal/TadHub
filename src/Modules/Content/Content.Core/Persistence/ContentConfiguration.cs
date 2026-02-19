using Content.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Content.Core.Persistence;

public class BlogPostConfiguration : IEntityTypeConfiguration<BlogPost>
{
    public void Configure(EntityTypeBuilder<BlogPost> builder)
    {
        builder.ToTable("blog_posts");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Slug).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Excerpt).HasMaxLength(1024);
        builder.Property(x => x.Content).HasColumnType("text");
        builder.Property(x => x.Status).HasMaxLength(32);
        builder.Property(x => x.FeaturedImageUrl).HasMaxLength(512);
        builder.Property(x => x.SeoTitle).HasMaxLength(256);
        builder.Property(x => x.SeoDescription).HasMaxLength(512);
        builder.HasIndex(x => new { x.TenantId, x.Slug }).IsUnique().HasDatabaseName("ix_blog_posts_tenant_slug");
        builder.HasIndex(x => new { x.TenantId, x.Status, x.PublishedAt }).HasDatabaseName("ix_blog_posts_tenant_status");
        builder.HasOne(x => x.Category).WithMany(x => x.Posts).HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.SetNull);
        builder.HasMany(x => x.Tags).WithOne(x => x.BlogPost).HasForeignKey(x => x.BlogPostId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class BlogCategoryConfiguration : IEntityTypeConfiguration<BlogCategory>
{
    public void Configure(EntityTypeBuilder<BlogCategory> builder)
    {
        builder.ToTable("blog_categories");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Slug).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(512);
        builder.HasIndex(x => new { x.TenantId, x.Slug }).IsUnique().HasDatabaseName("ix_blog_categories_tenant_slug");
    }
}

public class BlogTagConfiguration : IEntityTypeConfiguration<BlogTag>
{
    public void Configure(EntityTypeBuilder<BlogTag> builder)
    {
        builder.ToTable("blog_tags");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Slug).HasMaxLength(64).IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.Slug }).IsUnique().HasDatabaseName("ix_blog_tags_tenant_slug");
    }
}

public class BlogPostTagConfiguration : IEntityTypeConfiguration<BlogPostTag>
{
    public void Configure(EntityTypeBuilder<BlogPostTag> builder)
    {
        builder.ToTable("blog_post_tags");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.BlogPostId, x.BlogTagId }).IsUnique().HasDatabaseName("ix_blog_post_tags_post_tag");
        builder.HasOne(x => x.BlogTag).WithMany(x => x.Posts).HasForeignKey(x => x.BlogTagId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class KnowledgeBaseConfiguration : IEntityTypeConfiguration<KnowledgeBase>
{
    public void Configure(EntityTypeBuilder<KnowledgeBase> builder)
    {
        builder.ToTable("knowledge_bases");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Slug).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1024);
        builder.Property(x => x.DefaultLanguage).HasMaxLength(8);
        builder.HasIndex(x => new { x.TenantId, x.Slug }).IsUnique().HasDatabaseName("ix_knowledge_bases_tenant_slug");
        builder.HasMany(x => x.Categories).WithOne(x => x.KnowledgeBase).HasForeignKey(x => x.KnowledgeBaseId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class KBCategoryConfiguration : IEntityTypeConfiguration<KBCategory>
{
    public void Configure(EntityTypeBuilder<KBCategory> builder)
    {
        builder.ToTable("kb_categories");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Slug).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(512);
        builder.Property(x => x.Icon).HasMaxLength(64);
        builder.HasIndex(x => new { x.KnowledgeBaseId, x.Slug }).IsUnique().HasDatabaseName("ix_kb_categories_kb_slug");
        builder.HasOne(x => x.Parent).WithMany().HasForeignKey(x => x.ParentId).OnDelete(DeleteBehavior.SetNull);
        builder.HasMany(x => x.Articles).WithOne(x => x.Category).HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.SetNull);
    }
}

public class KBArticleConfiguration : IEntityTypeConfiguration<KBArticle>
{
    public void Configure(EntityTypeBuilder<KBArticle> builder)
    {
        builder.ToTable("kb_articles");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Slug).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Content).HasColumnType("text");
        builder.Property(x => x.Language).HasMaxLength(8);
        builder.Property(x => x.Status).HasMaxLength(32);
        builder.HasIndex(x => new { x.KnowledgeBaseId, x.Language, x.Slug }).IsUnique().HasDatabaseName("ix_kb_articles_kb_lang_slug");
        builder.HasMany(x => x.Versions).WithOne(x => x.Article).HasForeignKey(x => x.ArticleId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class KBArticleVersionConfiguration : IEntityTypeConfiguration<KBArticleVersion>
{
    public void Configure(EntityTypeBuilder<KBArticleVersion> builder)
    {
        builder.ToTable("kb_article_versions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Content).HasColumnType("text");
        builder.Property(x => x.ChangeNote).HasMaxLength(512);
        builder.HasIndex(x => new { x.ArticleId, x.Version }).IsUnique().HasDatabaseName("ix_kb_article_versions_article_version");
    }
}

public class PageConfiguration : IEntityTypeConfiguration<Page>
{
    public void Configure(EntityTypeBuilder<Page> builder)
    {
        builder.ToTable("pages");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Slug).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Content).HasColumnType("text");
        builder.Property(x => x.Status).HasMaxLength(32);
        builder.Property(x => x.SeoTitle).HasMaxLength(256);
        builder.Property(x => x.SeoDescription).HasMaxLength(512);
        builder.HasIndex(x => new { x.TenantId, x.Slug }).IsUnique().HasDatabaseName("ix_pages_tenant_slug");
        builder.HasOne(x => x.Template).WithMany().HasForeignKey(x => x.TemplateId).OnDelete(DeleteBehavior.SetNull);
        builder.HasMany(x => x.Blocks).WithOne(x => x.Page).HasForeignKey(x => x.PageId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.Versions).WithOne(x => x.Page).HasForeignKey(x => x.PageId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class PageBlockConfiguration : IEntityTypeConfiguration<PageBlock>
{
    public void Configure(EntityTypeBuilder<PageBlock> builder)
    {
        builder.ToTable("page_blocks");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Type).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Content).HasColumnType("jsonb");
    }
}

public class PageVersionConfiguration : IEntityTypeConfiguration<PageVersion>
{
    public void Configure(EntityTypeBuilder<PageVersion> builder)
    {
        builder.ToTable("page_versions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Content).HasColumnType("text");
        builder.HasIndex(x => new { x.PageId, x.Version }).IsUnique().HasDatabaseName("ix_page_versions_page_version");
    }
}

public class TemplateConfiguration : IEntityTypeConfiguration<Template>
{
    public void Configure(EntityTypeBuilder<Template> builder)
    {
        builder.ToTable("templates");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(512);
        builder.Property(x => x.Schema).HasColumnType("jsonb");
        builder.Property(x => x.DefaultContent).HasColumnType("text");
    }
}

public class MediaConfiguration : IEntityTypeConfiguration<Media>
{
    public void Configure(EntityTypeBuilder<Media> builder)
    {
        builder.ToTable("media");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.FileName).HasMaxLength(256).IsRequired();
        builder.Property(x => x.OriginalFileName).HasMaxLength(256).IsRequired();
        builder.Property(x => x.MimeType).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Url).HasMaxLength(1024).IsRequired();
        builder.Property(x => x.ThumbnailUrl).HasMaxLength(1024);
        builder.Property(x => x.Alt).HasMaxLength(256);
        builder.Property(x => x.Caption).HasMaxLength(512);
        builder.HasIndex(x => new { x.TenantId, x.FolderId }).HasDatabaseName("ix_media_tenant_folder");
        builder.HasOne(x => x.Folder).WithMany(x => x.Files).HasForeignKey(x => x.FolderId).OnDelete(DeleteBehavior.SetNull);
    }
}

public class MediaFolderConfiguration : IEntityTypeConfiguration<MediaFolder>
{
    public void Configure(EntityTypeBuilder<MediaFolder> builder)
    {
        builder.ToTable("media_folders");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(128).IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.ParentId, x.Name }).IsUnique().HasDatabaseName("ix_media_folders_tenant_parent_name");
        builder.HasOne(x => x.Parent).WithMany(x => x.Children).HasForeignKey(x => x.ParentId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class ContentTranslationConfiguration : IEntityTypeConfiguration<ContentTranslation>
{
    public void Configure(EntityTypeBuilder<ContentTranslation> builder)
    {
        builder.ToTable("content_translations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EntityType).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Field).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Language).HasMaxLength(8).IsRequired();
        builder.Property(x => x.Value).HasColumnType("text").IsRequired();
        builder.HasIndex(x => new { x.EntityType, x.EntityId, x.Field, x.Language }).IsUnique().HasDatabaseName("ix_content_translations_entity_field_lang");
    }
}

public class ContentRevisionConfiguration : IEntityTypeConfiguration<ContentRevision>
{
    public void Configure(EntityTypeBuilder<ContentRevision> builder)
    {
        builder.ToTable("content_revisions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EntityType).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Data).HasColumnType("jsonb");
        builder.Property(x => x.Note).HasMaxLength(512);
        builder.HasIndex(x => new { x.EntityType, x.EntityId, x.Version }).IsUnique().HasDatabaseName("ix_content_revisions_entity_version");
    }
}
