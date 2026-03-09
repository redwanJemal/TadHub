using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaasKit.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAccommodationModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "accommodation_stays",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    stay_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    status_changed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    worker_id = table.Column<Guid>(type: "uuid", nullable: false),
                    placement_id = table.Column<Guid>(type: "uuid", nullable: true),
                    arrival_id = table.Column<Guid>(type: "uuid", nullable: true),
                    check_in_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    check_out_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    room = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    location = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    departure_reason = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    departure_notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    checked_in_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    checked_out_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
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
                    table.PrimaryKey("pk_accommodation_stays", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_accommodation_stays_tenant_checkin_date",
                table: "accommodation_stays",
                columns: new[] { "tenant_id", "check_in_date" });

            migrationBuilder.CreateIndex(
                name: "ix_accommodation_stays_tenant_code",
                table: "accommodation_stays",
                columns: new[] { "tenant_id", "stay_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_accommodation_stays_tenant_status",
                table: "accommodation_stays",
                columns: new[] { "tenant_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_accommodation_stays_tenant_worker",
                table: "accommodation_stays",
                columns: new[] { "tenant_id", "worker_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "accommodation_stays");
        }
    }
}
