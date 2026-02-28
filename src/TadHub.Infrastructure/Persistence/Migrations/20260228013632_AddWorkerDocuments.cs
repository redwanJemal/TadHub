using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaasKit.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkerDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "worker_documents",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    worker_id = table.Column<Guid>(type: "uuid", nullable: false),
                    document_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    document_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    issued_at = table.Column<DateOnly>(type: "date", nullable: true),
                    expires_at = table.Column<DateOnly>(type: "date", nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    issuing_authority = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    file_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_worker_documents", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_worker_documents_tenant_id_expires_at",
                table: "worker_documents",
                columns: new[] { "tenant_id", "expires_at" });

            migrationBuilder.CreateIndex(
                name: "ix_worker_documents_tenant_id_status",
                table: "worker_documents",
                columns: new[] { "tenant_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_worker_documents_tenant_id_worker_id",
                table: "worker_documents",
                columns: new[] { "tenant_id", "worker_id" });

            migrationBuilder.CreateIndex(
                name: "ix_worker_documents_tenant_worker_type_number",
                table: "worker_documents",
                columns: new[] { "tenant_id", "worker_id", "document_type", "document_number" },
                unique: true,
                filter: "is_deleted = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "worker_documents");
        }
    }
}
