using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaasKit.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCandidateRegistrationGaps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "location_type",
                table: "candidates",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "place_of_birth",
                table: "candidates",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_candidates_location_type",
                table: "candidates",
                column: "location_type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_candidates_location_type",
                table: "candidates");

            migrationBuilder.DropColumn(
                name: "location_type",
                table: "candidates");

            migrationBuilder.DropColumn(
                name: "place_of_birth",
                table: "candidates");
        }
    }
}
