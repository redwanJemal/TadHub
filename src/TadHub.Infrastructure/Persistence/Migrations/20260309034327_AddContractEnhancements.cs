using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaasKit.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddContractEnhancements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "guarantee_period_type",
                table: "contracts",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "returnee_case_id",
                table: "contracts",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "runaway_case_id",
                table: "contracts",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "termination_reason_type",
                table: "contracts",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "guarantee_period_type",
                table: "contracts");

            migrationBuilder.DropColumn(
                name: "returnee_case_id",
                table: "contracts");

            migrationBuilder.DropColumn(
                name: "runaway_case_id",
                table: "contracts");

            migrationBuilder.DropColumn(
                name: "termination_reason_type",
                table: "contracts");
        }
    }
}
