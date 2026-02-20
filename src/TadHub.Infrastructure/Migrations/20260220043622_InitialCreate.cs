using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaasKit.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "analytics_events",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    properties = table.Column<string>(type: "jsonb", nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    session_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_analytics_events", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "analytics_sessions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    session_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    started_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ended_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    page_view_count = table.Column<int>(type: "integer", nullable: false),
                    event_count = table.Column<int>(type: "integer", nullable: false),
                    first_page_url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    last_page_url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    referrer = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    user_agent = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_analytics_sessions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "api_keys",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    prefix = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    key_hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    permissions = table.Column<string>(type: "jsonb", nullable: true),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    last_used_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    request_count = table.Column<long>(type: "bigint", nullable: false),
                    rate_limit_per_minute = table.Column<int>(type: "integer", nullable: true),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_api_keys", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "audit_events",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    payload = table.Column<string>(type: "jsonb", nullable: true),
                    metadata = table.Column<string>(type: "jsonb", nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    ip_address = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_events", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "audit_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    action = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    entity_type = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    entity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    old_values = table.Column<string>(type: "jsonb", nullable: true),
                    new_values = table.Column<string>(type: "jsonb", nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_logs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "blog_categories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    slug = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_blog_categories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "blog_tags",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    slug = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_blog_tags", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "content_revisions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    entity_type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    entity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false),
                    data = table.Column<string>(type: "jsonb", nullable: true),
                    author_id = table.Column<Guid>(type: "uuid", nullable: false),
                    note = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_content_revisions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "content_translations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    entity_type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    entity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    field = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    language = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    value = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_content_translations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "credits",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    amount = table.Column<long>(type: "bigint", nullable: false),
                    balance = table.Column<long>(type: "bigint", nullable: false),
                    description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    reference_id = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    reference_type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    metadata = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_credits", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "daily_stats",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    page_views = table.Column<int>(type: "integer", nullable: false),
                    unique_visitors = table.Column<int>(type: "integer", nullable: false),
                    sessions = table.Column<int>(type: "integer", nullable: false),
                    events = table.Column<int>(type: "integer", nullable: false),
                    avg_session_duration_ms = table.Column<int>(type: "integer", nullable: false),
                    bounce_rate = table.Column<decimal>(type: "numeric", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_daily_stats", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "feature_flags",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    percentage = table.Column<int>(type: "integer", nullable: true),
                    allowed_plans = table.Column<string>(type: "jsonb", nullable: true),
                    allowed_tenant_ids = table.Column<string>(type: "jsonb", nullable: true),
                    enabled_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    disabled_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_feature_flags", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "groups",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_groups", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "knowledge_bases",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    slug = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    is_public = table.Column<bool>(type: "boolean", nullable: false),
                    default_language = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_knowledge_bases", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "media_folders",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    parent_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_media_folders", x => x.id);
                    table.ForeignKey(
                        name: "fk_media_folders_media_folders_parent_id",
                        column: x => x.parent_id,
                        principalTable: "media_folders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    body = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "info"),
                    link = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    is_read = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    read_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notifications", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "page_views",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    session_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    referrer = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    user_agent = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    ip_address = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    country = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    city = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    device = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    browser = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    duration_ms = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_page_views", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "permissions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    module = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_permissions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "plans",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    slug = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_default = table.Column<bool>(type: "boolean", nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    stripe_product_id = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_plans", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "portals",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    subdomain = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    primary_color = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    secondary_color = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    logo_url = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    favicon_url = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    custom_css = table.Column<string>(type: "character varying(50000)", maxLength: 50000, nullable: true),
                    seo_title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    seo_description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    seo_keywords = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    og_image_url = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    allow_public_registration = table.Column<bool>(type: "boolean", nullable: false),
                    require_email_verification = table.Column<bool>(type: "boolean", nullable: false),
                    enable_sso = table.Column<bool>(type: "boolean", nullable: false),
                    sso_config = table.Column<string>(type: "jsonb", nullable: true),
                    stripe_account_id = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    stripe_onboarding_complete = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_portals", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_default = table.Column<bool>(type: "boolean", nullable: false),
                    is_system = table.Column<bool>(type: "boolean", nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "template_entities",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_template_entities", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "templates",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    schema = table.Column<string>(type: "jsonb", nullable: true),
                    default_content = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_templates", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tenant_types",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tenant_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user_profiles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    keycloak_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    avatar_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    locale = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "en"),
                    default_tenant_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    last_login_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_profiles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "webhooks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    events = table.Column<string>(type: "jsonb", nullable: true),
                    secret = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    last_triggered_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    failure_count = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_webhooks", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "api_key_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    api_key_id = table.Column<Guid>(type: "uuid", nullable: false),
                    endpoint = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    method = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    status_code = table.Column<int>(type: "integer", nullable: false),
                    duration_ms = table.Column<int>(type: "integer", nullable: false),
                    ip_address = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    user_agent = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    request_size = table.Column<long>(type: "bigint", nullable: true),
                    response_size = table.Column<long>(type: "bigint", nullable: true),
                    error_message = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_api_key_logs", x => x.id);
                    table.ForeignKey(
                        name: "fk_api_key_logs_api_keys_api_key_id",
                        column: x => x.api_key_id,
                        principalTable: "api_keys",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "blog_posts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    slug = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    excerpt = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    content = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    published_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    category_id = table.Column<Guid>(type: "uuid", nullable: true),
                    author_id = table.Column<Guid>(type: "uuid", nullable: false),
                    featured_image_url = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    seo_title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    seo_description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    view_count = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_blog_posts", x => x.id);
                    table.ForeignKey(
                        name: "fk_blog_posts_blog_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "blog_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "feature_flag_filters",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    feature_flag_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    value = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_feature_flag_filters", x => x.id);
                    table.ForeignKey(
                        name: "fk_feature_flag_filters_feature_flags_feature_flag_id",
                        column: x => x.feature_flag_id,
                        principalTable: "feature_flags",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "kb_categories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    knowledge_base_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    slug = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    icon = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    parent_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_kb_categories", x => x.id);
                    table.ForeignKey(
                        name: "fk_kb_categories_kb_categories_parent_id",
                        column: x => x.parent_id,
                        principalTable: "kb_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_kb_categories_knowledge_base_knowledge_base_id",
                        column: x => x.knowledge_base_id,
                        principalTable: "knowledge_bases",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "media",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    file_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    original_file_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    mime_type = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    size = table.Column<long>(type: "bigint", nullable: false),
                    url = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    thumbnail_url = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    folder_id = table.Column<Guid>(type: "uuid", nullable: true),
                    width = table.Column<int>(type: "integer", nullable: true),
                    height = table.Column<int>(type: "integer", nullable: true),
                    alt = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    caption = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    uploaded_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_media", x => x.id);
                    table.ForeignKey(
                        name: "fk_media_media_folder_folder_id",
                        column: x => x.folder_id,
                        principalTable: "media_folders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "plan_features",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    plan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    value_type = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    boolean_value = table.Column<bool>(type: "boolean", nullable: true),
                    numeric_value = table.Column<long>(type: "bigint", nullable: true),
                    is_unlimited = table.Column<bool>(type: "boolean", nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_plan_features", x => x.id);
                    table.ForeignKey(
                        name: "fk_plan_features_plans_plan_id",
                        column: x => x.plan_id,
                        principalTable: "plans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "plan_prices",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    plan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount = table.Column<long>(type: "bigint", nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    interval = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    interval_count = table.Column<int>(type: "integer", nullable: false),
                    trial_days = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    stripe_price_id = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_plan_prices", x => x.id);
                    table.ForeignKey(
                        name: "fk_plan_prices_plans_plan_id",
                        column: x => x.plan_id,
                        principalTable: "plans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "plan_usage_based_prices",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    plan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    metric_key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    unit = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    price_per_unit = table.Column<long>(type: "bigint", nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    included_units = table.Column<long>(type: "bigint", nullable: false),
                    stripe_price_id = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_plan_usage_based_prices", x => x.id);
                    table.ForeignKey(
                        name: "fk_plan_usage_based_prices_plans_plan_id",
                        column: x => x.plan_id,
                        principalTable: "plans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "portal_api_keys",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    portal_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    key_prefix = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    key_hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_used_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    usage_count = table.Column<long>(type: "bigint", nullable: false),
                    scopes = table.Column<string>(type: "jsonb", nullable: true),
                    allowed_ips = table.Column<string>(type: "jsonb", nullable: true),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_portal_api_keys", x => x.id);
                    table.ForeignKey(
                        name: "fk_portal_api_keys_portal_portal_id",
                        column: x => x.portal_id,
                        principalTable: "portals",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "portal_domains",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    portal_id = table.Column<Guid>(type: "uuid", nullable: false),
                    domain = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    is_primary = table.Column<bool>(type: "boolean", nullable: false),
                    is_verified = table.Column<bool>(type: "boolean", nullable: false),
                    verification_token = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    verified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ssl_status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "pending"),
                    ssl_expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_portal_domains", x => x.id);
                    table.ForeignKey(
                        name: "fk_portal_domains_portals_portal_id",
                        column: x => x.portal_id,
                        principalTable: "portals",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "portal_pages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    portal_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    slug = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    content = table.Column<string>(type: "text", nullable: true),
                    content_format = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false, defaultValue: "html"),
                    is_published = table.Column<bool>(type: "boolean", nullable: false),
                    published_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    show_in_navigation = table.Column<bool>(type: "boolean", nullable: false),
                    page_type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "content"),
                    seo_title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    seo_description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    featured_image_url = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_portal_pages", x => x.id);
                    table.ForeignKey(
                        name: "fk_portal_pages_portals_portal_id",
                        column: x => x.portal_id,
                        principalTable: "portals",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "portal_settings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    portal_id = table.Column<Guid>(type: "uuid", nullable: false),
                    default_language = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false, defaultValue: "en"),
                    timezone = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, defaultValue: "UTC"),
                    date_format = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    support_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    from_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    from_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    twitter_url = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    linked_in_url = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    facebook_url = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    instagram_url = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    terms_url = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    privacy_url = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    google_analytics_id = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    custom_tracking_scripts = table.Column<string>(type: "jsonb", nullable: true),
                    additional_settings = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_portal_settings", x => x.id);
                    table.ForeignKey(
                        name: "fk_portal_settings_portal_portal_id",
                        column: x => x.portal_id,
                        principalTable: "portals",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "portal_themes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    portal_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    base_template = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "default"),
                    variables = table.Column<string>(type: "jsonb", nullable: true),
                    custom_css = table.Column<string>(type: "text", nullable: true),
                    custom_js = table.Column<string>(type: "text", nullable: true),
                    header_template = table.Column<string>(type: "text", nullable: true),
                    footer_template = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_portal_themes", x => x.id);
                    table.ForeignKey(
                        name: "fk_portal_themes_portals_portal_id",
                        column: x => x.portal_id,
                        principalTable: "portals",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "portal_users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    portal_id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    normalized_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    email_verified = table.Column<bool>(type: "boolean", nullable: false),
                    password_hash = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    first_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    last_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    display_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    avatar_url = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    phone_number = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    last_login_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    login_count = table.Column<int>(type: "integer", nullable: false),
                    sso_subject = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    sso_provider = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    metadata = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_portal_users", x => x.id);
                    table.ForeignKey(
                        name: "fk_portal_users_portals_portal_id",
                        column: x => x.portal_id,
                        principalTable: "portals",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "group_roles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    group_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_group_roles", x => x.id);
                    table.ForeignKey(
                        name: "fk_group_roles_groups_group_id",
                        column: x => x.group_id,
                        principalTable: "groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_group_roles_role_role_id",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "role_permissions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    permission_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_role_permissions", x => x.id);
                    table.ForeignKey(
                        name: "fk_role_permissions_permissions_permission_id",
                        column: x => x.permission_id,
                        principalTable: "permissions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_role_permissions_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "pages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    slug = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    content = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    published_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    seo_title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    seo_description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    template_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pages", x => x.id);
                    table.ForeignKey(
                        name: "fk_pages_template_template_id",
                        column: x => x.template_id,
                        principalTable: "templates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "tenant_type_relationships",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    parent_type_id = table.Column<Guid>(type: "uuid", nullable: false),
                    child_type_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tenant_type_relationships", x => x.id);
                    table.ForeignKey(
                        name: "fk_tenant_type_relationships_tenant_types_child_type_id",
                        column: x => x.child_type_id,
                        principalTable: "tenant_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_tenant_type_relationships_tenant_types_parent_type_id",
                        column: x => x.parent_type_id,
                        principalTable: "tenant_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tenants",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    slug = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    tenant_type_id = table.Column<Guid>(type: "uuid", nullable: true),
                    logo_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    website = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    settings = table.Column<string>(type: "jsonb", nullable: true),
                    parent_tenant_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tadbeer_license_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    mohre_license_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    trade_license_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    trade_license_expiry = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    emirate = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    license_expiry_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    tax_registration_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tenants", x => x.id);
                    table.ForeignKey(
                        name: "fk_tenants_tenant_type_tenant_type_id",
                        column: x => x.tenant_type_id,
                        principalTable: "tenant_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_tenants_tenants_parent_tenant_id",
                        column: x => x.parent_tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "admin_users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_super_admin = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_admin_users", x => x.id);
                    table.ForeignKey(
                        name: "fk_admin_users_user_profile_user_id",
                        column: x => x.user_id,
                        principalTable: "user_profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "group_users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    group_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    joined_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_group_users", x => x.id);
                    table.ForeignKey(
                        name: "fk_group_users_groups_group_id",
                        column: x => x.group_id,
                        principalTable: "groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_group_users_user_profiles_user_id",
                        column: x => x.user_id,
                        principalTable: "user_profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_roles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    assigned_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    assigned_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_roles", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_roles_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_user_roles_user_profiles_user_id",
                        column: x => x.user_id,
                        principalTable: "user_profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "webhook_deliveries",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    webhook_id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    payload = table.Column<string>(type: "jsonb", nullable: true),
                    status_code = table.Column<int>(type: "integer", nullable: true),
                    attempts = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    delivered_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    error_message = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_webhook_deliveries", x => x.id);
                    table.ForeignKey(
                        name: "fk_webhook_deliveries_webhooks_webhook_id",
                        column: x => x.webhook_id,
                        principalTable: "webhooks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "blog_post_tags",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    blog_post_id = table.Column<Guid>(type: "uuid", nullable: false),
                    blog_tag_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_blog_post_tags", x => x.id);
                    table.ForeignKey(
                        name: "fk_blog_post_tags_blog_posts_blog_post_id",
                        column: x => x.blog_post_id,
                        principalTable: "blog_posts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_blog_post_tags_blog_tag_blog_tag_id",
                        column: x => x.blog_tag_id,
                        principalTable: "blog_tags",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "kb_articles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    knowledge_base_id = table.Column<Guid>(type: "uuid", nullable: false),
                    category_id = table.Column<Guid>(type: "uuid", nullable: true),
                    title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    slug = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    content = table.Column<string>(type: "text", nullable: true),
                    language = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    published_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    author_id = table.Column<Guid>(type: "uuid", nullable: false),
                    view_count = table.Column<int>(type: "integer", nullable: false),
                    helpful_count = table.Column<int>(type: "integer", nullable: false),
                    not_helpful_count = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_kb_articles", x => x.id);
                    table.ForeignKey(
                        name: "fk_kb_articles_kb_category_category_id",
                        column: x => x.category_id,
                        principalTable: "kb_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "checkout_sessions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    stripe_session_id = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    plan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    plan_price_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    completed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    initiated_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_checkout_sessions", x => x.id);
                    table.ForeignKey(
                        name: "fk_checkout_sessions_plan_plan_id",
                        column: x => x.plan_id,
                        principalTable: "plans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_checkout_sessions_plan_price_plan_price_id",
                        column: x => x.plan_price_id,
                        principalTable: "plan_prices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tenant_subscriptions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    plan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    plan_price_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    stripe_subscription_id = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    stripe_customer_id = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    current_period_start = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    current_period_end = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    trial_end = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    canceled_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    cancel_at_period_end = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tenant_subscriptions", x => x.id);
                    table.ForeignKey(
                        name: "fk_tenant_subscriptions_plan_prices_plan_price_id",
                        column: x => x.plan_price_id,
                        principalTable: "plan_prices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_tenant_subscriptions_plans_plan_id",
                        column: x => x.plan_id,
                        principalTable: "plans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "portal_invitations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    portal_id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    token = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    accepted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    invited_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    portal_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    message = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_portal_invitations", x => x.id);
                    table.ForeignKey(
                        name: "fk_portal_invitations_portal_user_portal_user_id",
                        column: x => x.portal_user_id,
                        principalTable: "portal_users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_portal_invitations_portals_portal_id",
                        column: x => x.portal_id,
                        principalTable: "portals",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "portal_subscriptions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    portal_id = table.Column<Guid>(type: "uuid", nullable: false),
                    portal_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    plan_name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    stripe_subscription_id = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    stripe_customer_id = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    current_period_start = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    current_period_end = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    trial_end = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    canceled_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    cancel_at_period_end = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_portal_subscriptions", x => x.id);
                    table.ForeignKey(
                        name: "fk_portal_subscriptions_portal_user_portal_user_id",
                        column: x => x.portal_user_id,
                        principalTable: "portal_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_portal_subscriptions_portals_portal_id",
                        column: x => x.portal_id,
                        principalTable: "portals",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "portal_user_registrations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    portal_id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    first_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    last_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    verification_token = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    verified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    portal_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    ip_address = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    user_agent = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_portal_user_registrations", x => x.id);
                    table.ForeignKey(
                        name: "fk_portal_user_registrations_portal_users_portal_user_id",
                        column: x => x.portal_user_id,
                        principalTable: "portal_users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_portal_user_registrations_portals_portal_id",
                        column: x => x.portal_id,
                        principalTable: "portals",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "page_blocks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    page_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    content = table.Column<string>(type: "jsonb", nullable: true),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_page_blocks", x => x.id);
                    table.ForeignKey(
                        name: "fk_page_blocks_page_page_id",
                        column: x => x.page_id,
                        principalTable: "pages",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "page_versions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    page_id = table.Column<Guid>(type: "uuid", nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false),
                    content = table.Column<string>(type: "text", nullable: true),
                    author_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_page_versions", x => x.id);
                    table.ForeignKey(
                        name: "fk_page_versions_pages_page_id",
                        column: x => x.page_id,
                        principalTable: "pages",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "shared_pool_agreements",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    from_tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    to_tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    approved_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    revoked_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    revenue_split_percentage = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    agreement_document_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_shared_pool_agreements", x => x.id);
                    table.CheckConstraint("ck_shared_pool_agreements_different_tenants", "from_tenant_id <> to_tenant_id");
                    table.CheckConstraint("ck_shared_pool_agreements_revenue_split", "revenue_split_percentage >= 0 AND revenue_split_percentage <= 100");
                    table.ForeignKey(
                        name: "fk_shared_pool_agreements_tenants_from_tenant_id",
                        column: x => x.from_tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_shared_pool_agreements_tenants_to_tenant_id",
                        column: x => x.to_tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tadbeer_licenses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    license_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    issued_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    document_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tadbeer_licenses", x => x.id);
                    table.ForeignKey(
                        name: "fk_tadbeer_licenses_tenant_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tenant_user_invitations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    token = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    accepted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    invited_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tenant_user_invitations", x => x.id);
                    table.ForeignKey(
                        name: "fk_tenant_user_invitations_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_tenant_user_invitations_user_profiles_invited_by_user_id",
                        column: x => x.invited_by_user_id,
                        principalTable: "user_profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tenant_users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    joined_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tenant_users", x => x.id);
                    table.ForeignKey(
                        name: "fk_tenant_users_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_tenant_users_user_profiles_user_id",
                        column: x => x.user_id,
                        principalTable: "user_profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "kb_article_versions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    article_id = table.Column<Guid>(type: "uuid", nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    content = table.Column<string>(type: "text", nullable: true),
                    author_id = table.Column<Guid>(type: "uuid", nullable: false),
                    change_note = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_kb_article_versions", x => x.id);
                    table.ForeignKey(
                        name: "fk_kb_article_versions_kb_articles_article_id",
                        column: x => x.article_id,
                        principalTable: "kb_articles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tenant_subscription_products",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_subscription_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    stripe_subscription_item_id = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tenant_subscription_products", x => x.id);
                    table.ForeignKey(
                        name: "fk_tenant_subscription_products_tenant_subscriptions_tenant_su",
                        column: x => x.tenant_subscription_id,
                        principalTable: "tenant_subscriptions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tenant_usage_records",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_subscription_id = table.Column<Guid>(type: "uuid", nullable: false),
                    metric_key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    quantity = table.Column<long>(type: "bigint", nullable: false),
                    period_start = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    period_end = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    reported_to_stripe = table.Column<bool>(type: "boolean", nullable: false),
                    stripe_usage_record_id = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tenant_usage_records", x => x.id);
                    table.ForeignKey(
                        name: "fk_tenant_usage_records_tenant_subscriptions_tenant_subscripti",
                        column: x => x.tenant_subscription_id,
                        principalTable: "tenant_subscriptions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "shared_pool_workers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    shared_pool_agreement_id = table.Column<Guid>(type: "uuid", nullable: false),
                    worker_id = table.Column<Guid>(type: "uuid", nullable: false),
                    shared_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    revoked_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_shared_pool_workers", x => x.id);
                    table.ForeignKey(
                        name: "fk_shared_pool_workers_shared_pool_agreements_shared_pool_agre",
                        column: x => x.shared_pool_agreement_id,
                        principalTable: "shared_pool_agreements",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tenant_subscription_prices",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_subscription_product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    stripe_price_id = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    amount = table.Column<long>(type: "bigint", nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    interval = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tenant_subscription_prices", x => x.id);
                    table.ForeignKey(
                        name: "fk_tenant_subscription_prices_tenant_subscription_product_tena",
                        column: x => x.tenant_subscription_product_id,
                        principalTable: "tenant_subscription_products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_admin_users_user_id",
                table: "admin_users",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_analytics_events_tenant_name_created",
                table: "analytics_events",
                columns: new[] { "tenant_id", "name", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_analytics_sessions_tenant_session",
                table: "analytics_sessions",
                columns: new[] { "tenant_id", "session_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_api_key_logs_key_created",
                table: "api_key_logs",
                columns: new[] { "api_key_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_api_key_logs_tenant_status_created",
                table: "api_key_logs",
                columns: new[] { "tenant_id", "status_code", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_api_keys_hash",
                table: "api_keys",
                column: "key_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_api_keys_tenant_active",
                table: "api_keys",
                columns: new[] { "tenant_id", "is_active" });

            migrationBuilder.CreateIndex(
                name: "ix_audit_events_tenant_name_created",
                table: "audit_events",
                columns: new[] { "tenant_id", "event_name", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_tenant_entity_action",
                table: "audit_logs",
                columns: new[] { "tenant_id", "entity_type", "action", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_blog_categories_tenant_slug",
                table: "blog_categories",
                columns: new[] { "tenant_id", "slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_blog_post_tags_blog_tag_id",
                table: "blog_post_tags",
                column: "blog_tag_id");

            migrationBuilder.CreateIndex(
                name: "ix_blog_post_tags_post_tag",
                table: "blog_post_tags",
                columns: new[] { "blog_post_id", "blog_tag_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_blog_posts_category_id",
                table: "blog_posts",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_blog_posts_tenant_slug",
                table: "blog_posts",
                columns: new[] { "tenant_id", "slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_blog_posts_tenant_status",
                table: "blog_posts",
                columns: new[] { "tenant_id", "status", "published_at" });

            migrationBuilder.CreateIndex(
                name: "ix_blog_tags_tenant_slug",
                table: "blog_tags",
                columns: new[] { "tenant_id", "slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_checkout_sessions_plan_id",
                table: "checkout_sessions",
                column: "plan_id");

            migrationBuilder.CreateIndex(
                name: "ix_checkout_sessions_plan_price_id",
                table: "checkout_sessions",
                column: "plan_price_id");

            migrationBuilder.CreateIndex(
                name: "ix_checkout_sessions_stripe_id",
                table: "checkout_sessions",
                column: "stripe_session_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_checkout_sessions_tenant_status",
                table: "checkout_sessions",
                columns: new[] { "tenant_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_content_revisions_entity_version",
                table: "content_revisions",
                columns: new[] { "entity_type", "entity_id", "version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_content_translations_entity_field_lang",
                table: "content_translations",
                columns: new[] { "entity_type", "entity_id", "field", "language" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_credits_expires_at",
                table: "credits",
                column: "expires_at",
                filter: "expires_at IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_credits_tenant_created",
                table: "credits",
                columns: new[] { "tenant_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_credits_tenant_type_created",
                table: "credits",
                columns: new[] { "tenant_id", "type", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_daily_stats_tenant_date",
                table: "daily_stats",
                columns: new[] { "tenant_id", "date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_feature_flag_filters_feature_flag_id",
                table: "feature_flag_filters",
                column: "feature_flag_id");

            migrationBuilder.CreateIndex(
                name: "ix_feature_flags_name",
                table: "feature_flags",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_group_roles_group_role",
                table: "group_roles",
                columns: new[] { "group_id", "role_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_group_roles_role_id",
                table: "group_roles",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "ix_group_users_group_user",
                table: "group_users",
                columns: new[] { "group_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_group_users_user_id",
                table: "group_users",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_groups_tenant_id",
                table: "groups",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_groups_tenant_name",
                table: "groups",
                columns: new[] { "tenant_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_kb_article_versions_article_version",
                table: "kb_article_versions",
                columns: new[] { "article_id", "version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_kb_articles_category_id",
                table: "kb_articles",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_kb_articles_kb_lang_slug",
                table: "kb_articles",
                columns: new[] { "knowledge_base_id", "language", "slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_kb_categories_kb_slug",
                table: "kb_categories",
                columns: new[] { "knowledge_base_id", "slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_kb_categories_parent_id",
                table: "kb_categories",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "ix_knowledge_bases_tenant_slug",
                table: "knowledge_bases",
                columns: new[] { "tenant_id", "slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_media_folder_id",
                table: "media",
                column: "folder_id");

            migrationBuilder.CreateIndex(
                name: "ix_media_tenant_folder",
                table: "media",
                columns: new[] { "tenant_id", "folder_id" });

            migrationBuilder.CreateIndex(
                name: "ix_media_folders_parent_id",
                table: "media_folders",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "ix_media_folders_tenant_parent_name",
                table: "media_folders",
                columns: new[] { "tenant_id", "parent_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_notifications_tenant_user_read_created",
                table: "notifications",
                columns: new[] { "tenant_id", "user_id", "is_read", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_notifications_tenant_user_unread",
                table: "notifications",
                columns: new[] { "tenant_id", "user_id", "is_read" },
                filter: "is_read = false");

            migrationBuilder.CreateIndex(
                name: "ix_page_blocks_page_id",
                table: "page_blocks",
                column: "page_id");

            migrationBuilder.CreateIndex(
                name: "ix_page_versions_page_version",
                table: "page_versions",
                columns: new[] { "page_id", "version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_page_views_tenant_created",
                table: "page_views",
                columns: new[] { "tenant_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_page_views_tenant_url_created",
                table: "page_views",
                columns: new[] { "tenant_id", "url", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_pages_template_id",
                table: "pages",
                column: "template_id");

            migrationBuilder.CreateIndex(
                name: "ix_pages_tenant_slug",
                table: "pages",
                columns: new[] { "tenant_id", "slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_permissions_module",
                table: "permissions",
                column: "module");

            migrationBuilder.CreateIndex(
                name: "ix_permissions_name",
                table: "permissions",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_plan_features_plan_key",
                table: "plan_features",
                columns: new[] { "plan_id", "key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_plan_prices_plan_interval_currency",
                table: "plan_prices",
                columns: new[] { "plan_id", "interval", "currency" });

            migrationBuilder.CreateIndex(
                name: "ix_plan_usage_prices_plan_metric",
                table: "plan_usage_based_prices",
                columns: new[] { "plan_id", "metric_key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_plans_active",
                table: "plans",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_plans_slug",
                table: "plans",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_portal_api_keys_hash",
                table: "portal_api_keys",
                column: "key_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_portal_api_keys_portal_active",
                table: "portal_api_keys",
                columns: new[] { "portal_id", "is_active" });

            migrationBuilder.CreateIndex(
                name: "ix_portal_domains_domain",
                table: "portal_domains",
                column: "domain",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_portal_domains_portal",
                table: "portal_domains",
                column: "portal_id");

            migrationBuilder.CreateIndex(
                name: "ix_portal_invitations_portal_email",
                table: "portal_invitations",
                columns: new[] { "portal_id", "email", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_portal_invitations_portal_user_id",
                table: "portal_invitations",
                column: "portal_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_portal_invitations_token",
                table: "portal_invitations",
                column: "token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_portal_pages_portal_slug",
                table: "portal_pages",
                columns: new[] { "portal_id", "slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_portal_settings_portal",
                table: "portal_settings",
                column: "portal_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_portal_subscriptions_portal_id",
                table: "portal_subscriptions",
                column: "portal_id");

            migrationBuilder.CreateIndex(
                name: "ix_portal_subscriptions_stripe",
                table: "portal_subscriptions",
                column: "stripe_subscription_id");

            migrationBuilder.CreateIndex(
                name: "ix_portal_subscriptions_user",
                table: "portal_subscriptions",
                column: "portal_user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_portal_themes_portal_active",
                table: "portal_themes",
                columns: new[] { "portal_id", "is_active" });

            migrationBuilder.CreateIndex(
                name: "ix_portal_user_registrations_portal_email",
                table: "portal_user_registrations",
                columns: new[] { "portal_id", "email", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_portal_user_registrations_portal_user_id",
                table: "portal_user_registrations",
                column: "portal_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_portal_user_registrations_token",
                table: "portal_user_registrations",
                column: "verification_token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_portal_users_portal_email",
                table: "portal_users",
                columns: new[] { "portal_id", "normalized_email" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_portal_users_sso",
                table: "portal_users",
                columns: new[] { "portal_id", "sso_provider", "sso_subject" },
                filter: "sso_subject IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_portals_subdomain",
                table: "portals",
                column: "subdomain",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_portals_tenant_active",
                table: "portals",
                columns: new[] { "tenant_id", "is_active" });

            migrationBuilder.CreateIndex(
                name: "ix_role_permissions_permission_id",
                table: "role_permissions",
                column: "permission_id");

            migrationBuilder.CreateIndex(
                name: "ix_role_permissions_role_permission",
                table: "role_permissions",
                columns: new[] { "role_id", "permission_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_roles_is_default",
                table: "roles",
                column: "is_default");

            migrationBuilder.CreateIndex(
                name: "ix_roles_tenant_id",
                table: "roles",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_roles_tenant_name",
                table: "roles",
                columns: new[] { "tenant_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_shared_pool_agreements_from_tenant",
                table: "shared_pool_agreements",
                column: "from_tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_shared_pool_agreements_status",
                table: "shared_pool_agreements",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_shared_pool_agreements_tenant_pair_status",
                table: "shared_pool_agreements",
                columns: new[] { "from_tenant_id", "to_tenant_id", "status" },
                unique: true,
                filter: "status = 'Active'");

            migrationBuilder.CreateIndex(
                name: "ix_shared_pool_agreements_to_tenant",
                table: "shared_pool_agreements",
                column: "to_tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_shared_pool_workers_agreement",
                table: "shared_pool_workers",
                column: "shared_pool_agreement_id");

            migrationBuilder.CreateIndex(
                name: "ix_shared_pool_workers_agreement_active",
                table: "shared_pool_workers",
                columns: new[] { "shared_pool_agreement_id", "revoked_at" },
                filter: "revoked_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_shared_pool_workers_agreement_worker",
                table: "shared_pool_workers",
                columns: new[] { "shared_pool_agreement_id", "worker_id" },
                unique: true,
                filter: "revoked_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_shared_pool_workers_worker",
                table: "shared_pool_workers",
                column: "worker_id");

            migrationBuilder.CreateIndex(
                name: "ix_tadbeer_licenses_expires_at",
                table: "tadbeer_licenses",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "ix_tadbeer_licenses_status",
                table: "tadbeer_licenses",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_tadbeer_licenses_tenant_type",
                table: "tadbeer_licenses",
                columns: new[] { "tenant_id", "license_type" });

            migrationBuilder.CreateIndex(
                name: "ix_tadbeer_licenses_type_number",
                table: "tadbeer_licenses",
                columns: new[] { "license_type", "number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_template_entities_tenant_active",
                table: "template_entities",
                columns: new[] { "tenant_id", "is_active" });

            migrationBuilder.CreateIndex(
                name: "ix_template_entities_tenant_name",
                table: "template_entities",
                columns: new[] { "tenant_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tenant_subscription_prices_tenant_subscription_product_id",
                table: "tenant_subscription_prices",
                column: "tenant_subscription_product_id");

            migrationBuilder.CreateIndex(
                name: "ix_tenant_sub_products_subscription",
                table: "tenant_subscription_products",
                column: "tenant_subscription_id");

            migrationBuilder.CreateIndex(
                name: "ix_tenant_subscriptions_plan_id",
                table: "tenant_subscriptions",
                column: "plan_id");

            migrationBuilder.CreateIndex(
                name: "ix_tenant_subscriptions_plan_price_id",
                table: "tenant_subscriptions",
                column: "plan_price_id");

            migrationBuilder.CreateIndex(
                name: "ix_tenant_subscriptions_stripe_id",
                table: "tenant_subscriptions",
                column: "stripe_subscription_id");

            migrationBuilder.CreateIndex(
                name: "ix_tenant_subscriptions_tenant",
                table: "tenant_subscriptions",
                column: "tenant_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tenant_type_relationships_child_type_id",
                table: "tenant_type_relationships",
                column: "child_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_tenant_type_relationships_parent_child",
                table: "tenant_type_relationships",
                columns: new[] { "parent_type_id", "child_type_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tenant_types_display_order",
                table: "tenant_types",
                column: "display_order");

            migrationBuilder.CreateIndex(
                name: "ix_tenant_types_name",
                table: "tenant_types",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tenant_usage_records_tenant_metric_period",
                table: "tenant_usage_records",
                columns: new[] { "tenant_id", "metric_key", "period_start" });

            migrationBuilder.CreateIndex(
                name: "ix_tenant_usage_records_tenant_subscription_id",
                table: "tenant_usage_records",
                column: "tenant_subscription_id");

            migrationBuilder.CreateIndex(
                name: "ix_tenant_user_invitations_email",
                table: "tenant_user_invitations",
                column: "email");

            migrationBuilder.CreateIndex(
                name: "ix_tenant_user_invitations_expires_at",
                table: "tenant_user_invitations",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "ix_tenant_user_invitations_invited_by_user_id",
                table: "tenant_user_invitations",
                column: "invited_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_tenant_user_invitations_tenant_id",
                table: "tenant_user_invitations",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_tenant_user_invitations_token",
                table: "tenant_user_invitations",
                column: "token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tenant_users_role",
                table: "tenant_users",
                column: "role");

            migrationBuilder.CreateIndex(
                name: "ix_tenant_users_tenant_id",
                table: "tenant_users",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_tenant_users_tenant_user",
                table: "tenant_users",
                columns: new[] { "tenant_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tenant_users_user_id",
                table: "tenant_users",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_tenants_created_at",
                table: "tenants",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_tenants_emirate",
                table: "tenants",
                column: "emirate");

            migrationBuilder.CreateIndex(
                name: "ix_tenants_is_active",
                table: "tenants",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_tenants_parent_tenant_id",
                table: "tenants",
                column: "parent_tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_tenants_slug",
                table: "tenants",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tenants_status",
                table: "tenants",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_tenants_tadbeer_license",
                table: "tenants",
                column: "tadbeer_license_number",
                unique: true,
                filter: "tadbeer_license_number IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_tenants_tenant_type_id",
                table: "tenants",
                column: "tenant_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_profiles_created_at",
                table: "user_profiles",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_user_profiles_email",
                table: "user_profiles",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_profiles_is_active",
                table: "user_profiles",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_user_profiles_keycloak_id",
                table: "user_profiles",
                column: "keycloak_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_roles_role_id",
                table: "user_roles",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_roles_tenant_user",
                table: "user_roles",
                columns: new[] { "tenant_id", "user_id" });

            migrationBuilder.CreateIndex(
                name: "ix_user_roles_tenant_user_role",
                table: "user_roles",
                columns: new[] { "tenant_id", "user_id", "role_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_roles_user_id",
                table: "user_roles",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_webhook_deliveries_webhook_status",
                table: "webhook_deliveries",
                columns: new[] { "webhook_id", "status", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_webhooks_tenant_active",
                table: "webhooks",
                columns: new[] { "tenant_id", "is_active" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "admin_users");

            migrationBuilder.DropTable(
                name: "analytics_events");

            migrationBuilder.DropTable(
                name: "analytics_sessions");

            migrationBuilder.DropTable(
                name: "api_key_logs");

            migrationBuilder.DropTable(
                name: "audit_events");

            migrationBuilder.DropTable(
                name: "audit_logs");

            migrationBuilder.DropTable(
                name: "blog_post_tags");

            migrationBuilder.DropTable(
                name: "checkout_sessions");

            migrationBuilder.DropTable(
                name: "content_revisions");

            migrationBuilder.DropTable(
                name: "content_translations");

            migrationBuilder.DropTable(
                name: "credits");

            migrationBuilder.DropTable(
                name: "daily_stats");

            migrationBuilder.DropTable(
                name: "feature_flag_filters");

            migrationBuilder.DropTable(
                name: "group_roles");

            migrationBuilder.DropTable(
                name: "group_users");

            migrationBuilder.DropTable(
                name: "kb_article_versions");

            migrationBuilder.DropTable(
                name: "media");

            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.DropTable(
                name: "page_blocks");

            migrationBuilder.DropTable(
                name: "page_versions");

            migrationBuilder.DropTable(
                name: "page_views");

            migrationBuilder.DropTable(
                name: "plan_features");

            migrationBuilder.DropTable(
                name: "plan_usage_based_prices");

            migrationBuilder.DropTable(
                name: "portal_api_keys");

            migrationBuilder.DropTable(
                name: "portal_domains");

            migrationBuilder.DropTable(
                name: "portal_invitations");

            migrationBuilder.DropTable(
                name: "portal_pages");

            migrationBuilder.DropTable(
                name: "portal_settings");

            migrationBuilder.DropTable(
                name: "portal_subscriptions");

            migrationBuilder.DropTable(
                name: "portal_themes");

            migrationBuilder.DropTable(
                name: "portal_user_registrations");

            migrationBuilder.DropTable(
                name: "role_permissions");

            migrationBuilder.DropTable(
                name: "shared_pool_workers");

            migrationBuilder.DropTable(
                name: "tadbeer_licenses");

            migrationBuilder.DropTable(
                name: "template_entities");

            migrationBuilder.DropTable(
                name: "tenant_subscription_prices");

            migrationBuilder.DropTable(
                name: "tenant_type_relationships");

            migrationBuilder.DropTable(
                name: "tenant_usage_records");

            migrationBuilder.DropTable(
                name: "tenant_user_invitations");

            migrationBuilder.DropTable(
                name: "tenant_users");

            migrationBuilder.DropTable(
                name: "user_roles");

            migrationBuilder.DropTable(
                name: "webhook_deliveries");

            migrationBuilder.DropTable(
                name: "api_keys");

            migrationBuilder.DropTable(
                name: "blog_posts");

            migrationBuilder.DropTable(
                name: "blog_tags");

            migrationBuilder.DropTable(
                name: "feature_flags");

            migrationBuilder.DropTable(
                name: "groups");

            migrationBuilder.DropTable(
                name: "kb_articles");

            migrationBuilder.DropTable(
                name: "media_folders");

            migrationBuilder.DropTable(
                name: "pages");

            migrationBuilder.DropTable(
                name: "portal_users");

            migrationBuilder.DropTable(
                name: "permissions");

            migrationBuilder.DropTable(
                name: "shared_pool_agreements");

            migrationBuilder.DropTable(
                name: "tenant_subscription_products");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "user_profiles");

            migrationBuilder.DropTable(
                name: "webhooks");

            migrationBuilder.DropTable(
                name: "blog_categories");

            migrationBuilder.DropTable(
                name: "kb_categories");

            migrationBuilder.DropTable(
                name: "templates");

            migrationBuilder.DropTable(
                name: "portals");

            migrationBuilder.DropTable(
                name: "tenants");

            migrationBuilder.DropTable(
                name: "tenant_subscriptions");

            migrationBuilder.DropTable(
                name: "knowledge_bases");

            migrationBuilder.DropTable(
                name: "tenant_types");

            migrationBuilder.DropTable(
                name: "plan_prices");

            migrationBuilder.DropTable(
                name: "plans");
        }
    }
}
