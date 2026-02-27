using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaasKit.Migrations
{
    /// <inheritdoc />
    public partial class ExpandWorkerLifecycle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "arrived_at",
                table: "workers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "flight_date",
                table: "workers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "location",
                table: "workers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "procurement_paid_at",
                table: "workers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_workers_tenant_id_location",
                table: "workers",
                columns: new[] { "tenant_id", "location" });

            // Data migration: rename old statuses to new lifecycle statuses
            // Order matters: Active → Available first, then Deployed → Active
            migrationBuilder.Sql("""
                UPDATE workers SET status = 'Available' WHERE status = 'Active';
                UPDATE workers SET status = 'Active' WHERE status = 'Deployed';
                UPDATE workers SET status = 'Active', notes = COALESCE(notes || E'\n', '') || '[Migrated from OnLeave]' WHERE status = 'OnLeave';
                """);

            // Same renames in status history
            migrationBuilder.Sql("""
                UPDATE worker_status_history SET from_status = 'Available' WHERE from_status = 'Active';
                UPDATE worker_status_history SET to_status = 'Available' WHERE to_status = 'Active';
                UPDATE worker_status_history SET from_status = 'Active' WHERE from_status = 'Deployed';
                UPDATE worker_status_history SET to_status = 'Active' WHERE to_status = 'Deployed';
                UPDATE worker_status_history SET from_status = 'Active' WHERE from_status = 'OnLeave';
                UPDATE worker_status_history SET to_status = 'Active' WHERE to_status = 'OnLeave';
                """);

            // All existing workers are in-country (they were created post-arrival in old system)
            migrationBuilder.Sql("""
                UPDATE workers SET location = 'InCountry' WHERE location = '' OR location IS NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_workers_tenant_id_location",
                table: "workers");

            migrationBuilder.DropColumn(
                name: "arrived_at",
                table: "workers");

            migrationBuilder.DropColumn(
                name: "flight_date",
                table: "workers");

            migrationBuilder.DropColumn(
                name: "location",
                table: "workers");

            migrationBuilder.DropColumn(
                name: "procurement_paid_at",
                table: "workers");
        }
    }
}
