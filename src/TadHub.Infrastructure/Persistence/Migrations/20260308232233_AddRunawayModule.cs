using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaasKit.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRunawayModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "runaway_cases",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    case_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    worker_id = table.Column<Guid>(type: "uuid", nullable: false),
                    contract_id = table.Column<Guid>(type: "uuid", nullable: false),
                    client_id = table.Column<Guid>(type: "uuid", nullable: false),
                    supplier_id = table.Column<Guid>(type: "uuid", nullable: true),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    status_changed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    reported_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    reported_by = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    last_known_location = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    police_report_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    police_report_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_within_guarantee = table.Column<bool>(type: "boolean", nullable: false),
                    guarantee_period_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    confirmed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    settled_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    closed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("pk_runaway_cases", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "runaway_case_status_history",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    runaway_case_id = table.Column<Guid>(type: "uuid", nullable: false),
                    from_status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    to_status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    changed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    changed_by = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_runaway_case_status_history", x => x.id);
                    table.ForeignKey(
                        name: "fk_runaway_case_status_history_runaway_case_runaway_case_id",
                        column: x => x.runaway_case_id,
                        principalTable: "runaway_cases",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "runaway_expenses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    runaway_case_id = table.Column<Guid>(type: "uuid", nullable: false),
                    expense_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    paid_by = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_runaway_expenses", x => x.id);
                    table.ForeignKey(
                        name: "fk_runaway_expenses_runaway_cases_runaway_case_id",
                        column: x => x.runaway_case_id,
                        principalTable: "runaway_cases",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_runaway_history_case",
                table: "runaway_case_status_history",
                columns: new[] { "runaway_case_id", "changed_at" });

            migrationBuilder.CreateIndex(
                name: "ix_runaway_history_tenant",
                table: "runaway_case_status_history",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_runaway_cases_tenant_client",
                table: "runaway_cases",
                columns: new[] { "tenant_id", "client_id" });

            migrationBuilder.CreateIndex(
                name: "ix_runaway_cases_tenant_code",
                table: "runaway_cases",
                columns: new[] { "tenant_id", "case_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_runaway_cases_tenant_contract",
                table: "runaway_cases",
                columns: new[] { "tenant_id", "contract_id" });

            migrationBuilder.CreateIndex(
                name: "ix_runaway_cases_tenant_status",
                table: "runaway_cases",
                columns: new[] { "tenant_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_runaway_cases_tenant_worker",
                table: "runaway_cases",
                columns: new[] { "tenant_id", "worker_id" });

            migrationBuilder.CreateIndex(
                name: "ix_runaway_expenses_case",
                table: "runaway_expenses",
                column: "runaway_case_id");

            migrationBuilder.CreateIndex(
                name: "ix_runaway_expenses_tenant",
                table: "runaway_expenses",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "runaway_case_status_history");

            migrationBuilder.DropTable(
                name: "runaway_expenses");

            migrationBuilder.DropTable(
                name: "runaway_cases");
        }
    }
}
