using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaasKit.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddInsideCountryPlacementFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "flow_type",
                table: "placements",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "status_changed_step_at",
                table: "placements",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "trial_id",
                table: "placements",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "trial_started_at",
                table: "placements",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "trial_succeeded_at",
                table: "placements",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "flow_type",
                table: "placements");

            migrationBuilder.DropColumn(
                name: "status_changed_step_at",
                table: "placements");

            migrationBuilder.DropColumn(
                name: "trial_id",
                table: "placements");

            migrationBuilder.DropColumn(
                name: "trial_started_at",
                table: "placements");

            migrationBuilder.DropColumn(
                name: "trial_succeeded_at",
                table: "placements");
        }
    }
}
