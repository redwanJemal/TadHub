using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaasKit.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCandidateSupplierNavigation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_candidates_tenant_supplier_tenant_supplier_id",
                table: "candidates");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "fk_candidates_tenant_supplier_tenant_supplier_id",
                table: "candidates",
                column: "tenant_supplier_id",
                principalTable: "tenant_suppliers",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
