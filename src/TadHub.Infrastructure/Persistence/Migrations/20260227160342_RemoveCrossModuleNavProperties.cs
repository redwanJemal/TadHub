using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaasKit.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCrossModuleNavProperties : Migration
    {
        /// <inheritdoc />
        /// No-op: CLR navigation properties removed but DB FK constraints kept for referential integrity.
        protected override void Up(MigrationBuilder migrationBuilder)
        {
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
