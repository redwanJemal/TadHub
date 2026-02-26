using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaasKit.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkerModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "workers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    candidate_id = table.Column<Guid>(type: "uuid", nullable: false),
                    worker_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    status_changed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    status_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    activated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    terminated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    termination_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    full_name_en = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    full_name_ar = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    nationality = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    date_of_birth = table.Column<DateOnly>(type: "date", nullable: true),
                    gender = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    passport_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    passport_expiry = table.Column<DateOnly>(type: "date", nullable: true),
                    phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    religion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    marital_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    education_level = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    job_category_id = table.Column<Guid>(type: "uuid", nullable: true),
                    experience_years = table.Column<int>(type: "integer", nullable: true),
                    monthly_salary = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    photo_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    video_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    passport_document_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    source_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    tenant_supplier_id = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("pk_workers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "worker_languages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    worker_id = table.Column<Guid>(type: "uuid", nullable: false),
                    language = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    proficiency_level = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_worker_languages", x => x.id);
                    table.ForeignKey(
                        name: "fk_worker_languages_workers_worker_id",
                        column: x => x.worker_id,
                        principalTable: "workers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "worker_skills",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    worker_id = table.Column<Guid>(type: "uuid", nullable: false),
                    skill_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    proficiency_level = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_worker_skills", x => x.id);
                    table.ForeignKey(
                        name: "fk_worker_skills_workers_worker_id",
                        column: x => x.worker_id,
                        principalTable: "workers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "worker_status_history",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    worker_id = table.Column<Guid>(type: "uuid", nullable: false),
                    from_status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    to_status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    changed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    changed_by = table.Column<Guid>(type: "uuid", nullable: true),
                    reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_worker_status_history", x => x.id);
                    table.ForeignKey(
                        name: "fk_worker_status_history_worker_worker_id",
                        column: x => x.worker_id,
                        principalTable: "workers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_worker_languages_tenant_id",
                table: "worker_languages",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_worker_languages_worker_id",
                table: "worker_languages",
                column: "worker_id");

            migrationBuilder.CreateIndex(
                name: "ix_worker_languages_worker_id_language",
                table: "worker_languages",
                columns: new[] { "worker_id", "language" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_worker_skills_tenant_id",
                table: "worker_skills",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_worker_skills_worker_id",
                table: "worker_skills",
                column: "worker_id");

            migrationBuilder.CreateIndex(
                name: "ix_worker_skills_worker_id_skill_name",
                table: "worker_skills",
                columns: new[] { "worker_id", "skill_name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_worker_status_history_tenant_id",
                table: "worker_status_history",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_worker_status_history_worker_id_changed_at",
                table: "worker_status_history",
                columns: new[] { "worker_id", "changed_at" });

            migrationBuilder.CreateIndex(
                name: "ix_workers_tenant_id_candidate_id",
                table: "workers",
                columns: new[] { "tenant_id", "candidate_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_workers_tenant_id_job_category_id",
                table: "workers",
                columns: new[] { "tenant_id", "job_category_id" });

            migrationBuilder.CreateIndex(
                name: "ix_workers_tenant_id_nationality",
                table: "workers",
                columns: new[] { "tenant_id", "nationality" });

            migrationBuilder.CreateIndex(
                name: "ix_workers_tenant_id_status",
                table: "workers",
                columns: new[] { "tenant_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_workers_tenant_id_tenant_supplier_id",
                table: "workers",
                columns: new[] { "tenant_id", "tenant_supplier_id" });

            migrationBuilder.CreateIndex(
                name: "ix_workers_tenant_id_worker_code",
                table: "workers",
                columns: new[] { "tenant_id", "worker_code" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "worker_languages");

            migrationBuilder.DropTable(
                name: "worker_skills");

            migrationBuilder.DropTable(
                name: "worker_status_history");

            migrationBuilder.DropTable(
                name: "workers");
        }
    }
}
