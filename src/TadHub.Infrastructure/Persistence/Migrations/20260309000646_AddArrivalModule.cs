using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaasKit.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddArrivalModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "arrivals",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    arrival_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    status_changed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    worker_id = table.Column<Guid>(type: "uuid", nullable: false),
                    placement_id = table.Column<Guid>(type: "uuid", nullable: false),
                    supplier_id = table.Column<Guid>(type: "uuid", nullable: true),
                    flight_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    airport_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    airport_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    scheduled_arrival_date = table.Column<DateOnly>(type: "date", nullable: false),
                    scheduled_arrival_time = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    actual_arrival_time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    pre_travel_photo_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    arrival_photo_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    driver_pickup_photo_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    driver_id = table.Column<Guid>(type: "uuid", nullable: true),
                    driver_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    driver_confirmed_pickup_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    accommodation_confirmed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    accommodation_confirmed_by = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    customer_picked_up = table.Column<bool>(type: "boolean", nullable: false),
                    customer_pickup_confirmed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("pk_arrivals", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "arrival_status_history",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    arrival_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("pk_arrival_status_history", x => x.id);
                    table.ForeignKey(
                        name: "fk_arrival_status_history_arrival_arrival_id",
                        column: x => x.arrival_id,
                        principalTable: "arrivals",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_arrival_history_arrival",
                table: "arrival_status_history",
                columns: new[] { "arrival_id", "changed_at" });

            migrationBuilder.CreateIndex(
                name: "ix_arrival_history_tenant_id",
                table: "arrival_status_history",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_arrivals_tenant_code",
                table: "arrivals",
                columns: new[] { "tenant_id", "arrival_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_arrivals_tenant_driver",
                table: "arrivals",
                columns: new[] { "tenant_id", "driver_id" });

            migrationBuilder.CreateIndex(
                name: "ix_arrivals_tenant_placement",
                table: "arrivals",
                columns: new[] { "tenant_id", "placement_id" });

            migrationBuilder.CreateIndex(
                name: "ix_arrivals_tenant_scheduled_date",
                table: "arrivals",
                columns: new[] { "tenant_id", "scheduled_arrival_date" });

            migrationBuilder.CreateIndex(
                name: "ix_arrivals_tenant_status",
                table: "arrivals",
                columns: new[] { "tenant_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_arrivals_tenant_worker",
                table: "arrivals",
                columns: new[] { "tenant_id", "worker_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "arrival_status_history");

            migrationBuilder.DropTable(
                name: "arrivals");
        }
    }
}
