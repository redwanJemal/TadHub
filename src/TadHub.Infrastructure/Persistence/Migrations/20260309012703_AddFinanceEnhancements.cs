using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaasKit.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFinanceEnhancements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "payment_type",
                table: "supplier_payments",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "placement_id",
                table: "supplier_payments",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "supplier_debits",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    debit_number = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    supplier_id = table.Column<Guid>(type: "uuid", nullable: false),
                    worker_id = table.Column<Guid>(type: "uuid", nullable: true),
                    contract_id = table.Column<Guid>(type: "uuid", nullable: true),
                    case_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    case_id = table.Column<Guid>(type: "uuid", nullable: true),
                    debit_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    settlement_payment_id = table.Column<Guid>(type: "uuid", nullable: true),
                    settled_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
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
                    table.PrimaryKey("pk_supplier_debits", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_supplier_payments_tenant_id_placement_id",
                table: "supplier_payments",
                columns: new[] { "tenant_id", "placement_id" });

            migrationBuilder.CreateIndex(
                name: "ix_supplier_debits_tenant_id_case_type_case_id",
                table: "supplier_debits",
                columns: new[] { "tenant_id", "case_type", "case_id" });

            migrationBuilder.CreateIndex(
                name: "ix_supplier_debits_tenant_id_contract_id",
                table: "supplier_debits",
                columns: new[] { "tenant_id", "contract_id" });

            migrationBuilder.CreateIndex(
                name: "ix_supplier_debits_tenant_id_debit_number",
                table: "supplier_debits",
                columns: new[] { "tenant_id", "debit_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_supplier_debits_tenant_id_status",
                table: "supplier_debits",
                columns: new[] { "tenant_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_supplier_debits_tenant_id_supplier_id",
                table: "supplier_debits",
                columns: new[] { "tenant_id", "supplier_id" });

            migrationBuilder.CreateIndex(
                name: "ix_supplier_debits_tenant_id_worker_id",
                table: "supplier_debits",
                columns: new[] { "tenant_id", "worker_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "supplier_debits");

            migrationBuilder.DropIndex(
                name: "ix_supplier_payments_tenant_id_placement_id",
                table: "supplier_payments");

            migrationBuilder.DropColumn(
                name: "payment_type",
                table: "supplier_payments");

            migrationBuilder.DropColumn(
                name: "placement_id",
                table: "supplier_payments");
        }
    }
}
