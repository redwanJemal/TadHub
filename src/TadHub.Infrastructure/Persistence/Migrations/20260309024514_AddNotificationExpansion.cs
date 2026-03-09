using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaasKit.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationExpansion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "event_type",
                table: "notifications",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "priority",
                table: "notifications",
                type: "character varying(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "normal");

            migrationBuilder.CreateTable(
                name: "notification_templates",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    title_en = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    title_ar = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    body_en = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    body_ar = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    default_priority = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false, defaultValue: "normal"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notification_templates", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user_notification_preferences",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    muted = table.Column<bool>(type: "boolean", nullable: false),
                    channels = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false, defaultValue: "in_app"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_notification_preferences", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_notification_templates_tenant_event",
                table: "notification_templates",
                columns: new[] { "tenant_id", "event_type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_notification_prefs_tenant_user_event",
                table: "user_notification_preferences",
                columns: new[] { "tenant_id", "user_id", "event_type" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "notification_templates");

            migrationBuilder.DropTable(
                name: "user_notification_preferences");

            migrationBuilder.DropColumn(
                name: "event_type",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "priority",
                table: "notifications");
        }
    }
}
