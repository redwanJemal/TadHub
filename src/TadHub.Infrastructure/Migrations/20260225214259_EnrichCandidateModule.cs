using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaasKit.Migrations
{
    /// <inheritdoc />
    public partial class EnrichCandidateModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "education_level",
                table: "candidates",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "experience_years",
                table: "candidates",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "job_category_id",
                table: "candidates",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "marital_status",
                table: "candidates",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "monthly_salary",
                table: "candidates",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "photo_url",
                table: "candidates",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "procurement_cost",
                table: "candidates",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "religion",
                table: "candidates",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "video_url",
                table: "candidates",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "candidate_languages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    candidate_id = table.Column<Guid>(type: "uuid", nullable: false),
                    language = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    proficiency_level = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_candidate_languages", x => x.id);
                    table.ForeignKey(
                        name: "fk_candidate_languages_candidates_candidate_id",
                        column: x => x.candidate_id,
                        principalTable: "candidates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "candidate_skills",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    candidate_id = table.Column<Guid>(type: "uuid", nullable: false),
                    skill_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    proficiency_level = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_candidate_skills", x => x.id);
                    table.ForeignKey(
                        name: "fk_candidate_skills_candidates_candidate_id",
                        column: x => x.candidate_id,
                        principalTable: "candidates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_candidates_job_category_id",
                table: "candidates",
                column: "job_category_id");

            migrationBuilder.CreateIndex(
                name: "ix_candidate_languages_candidate_id",
                table: "candidate_languages",
                column: "candidate_id");

            migrationBuilder.CreateIndex(
                name: "ix_candidate_languages_candidate_id_language",
                table: "candidate_languages",
                columns: new[] { "candidate_id", "language" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_candidate_languages_tenant_id",
                table: "candidate_languages",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_candidate_skills_candidate_id",
                table: "candidate_skills",
                column: "candidate_id");

            migrationBuilder.CreateIndex(
                name: "ix_candidate_skills_candidate_id_skill_name",
                table: "candidate_skills",
                columns: new[] { "candidate_id", "skill_name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_candidate_skills_tenant_id",
                table: "candidate_skills",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "candidate_languages");

            migrationBuilder.DropTable(
                name: "candidate_skills");

            migrationBuilder.DropIndex(
                name: "ix_candidates_job_category_id",
                table: "candidates");

            migrationBuilder.DropColumn(
                name: "education_level",
                table: "candidates");

            migrationBuilder.DropColumn(
                name: "experience_years",
                table: "candidates");

            migrationBuilder.DropColumn(
                name: "job_category_id",
                table: "candidates");

            migrationBuilder.DropColumn(
                name: "marital_status",
                table: "candidates");

            migrationBuilder.DropColumn(
                name: "monthly_salary",
                table: "candidates");

            migrationBuilder.DropColumn(
                name: "photo_url",
                table: "candidates");

            migrationBuilder.DropColumn(
                name: "procurement_cost",
                table: "candidates");

            migrationBuilder.DropColumn(
                name: "religion",
                table: "candidates");

            migrationBuilder.DropColumn(
                name: "video_url",
                table: "candidates");
        }
    }
}
