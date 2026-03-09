using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaasKit.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOutsideCountryPlacementFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "arrival_id",
                table: "placements",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "contract_created_at",
                table: "placements",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "deployed_at",
                table: "placements",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "emirates_id_application_id",
                table: "placements",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "emirates_id_started_at",
                table: "placements",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "employment_visa_application_id",
                table: "placements",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "employment_visa_started_at",
                table: "placements",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "full_payment_received_at",
                table: "placements",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "residence_visa_application_id",
                table: "placements",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "residence_visa_started_at",
                table: "placements",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "arrival_id",
                table: "placements");

            migrationBuilder.DropColumn(
                name: "contract_created_at",
                table: "placements");

            migrationBuilder.DropColumn(
                name: "deployed_at",
                table: "placements");

            migrationBuilder.DropColumn(
                name: "emirates_id_application_id",
                table: "placements");

            migrationBuilder.DropColumn(
                name: "emirates_id_started_at",
                table: "placements");

            migrationBuilder.DropColumn(
                name: "employment_visa_application_id",
                table: "placements");

            migrationBuilder.DropColumn(
                name: "employment_visa_started_at",
                table: "placements");

            migrationBuilder.DropColumn(
                name: "full_payment_received_at",
                table: "placements");

            migrationBuilder.DropColumn(
                name: "residence_visa_application_id",
                table: "placements");

            migrationBuilder.DropColumn(
                name: "residence_visa_started_at",
                table: "placements");
        }
    }
}
