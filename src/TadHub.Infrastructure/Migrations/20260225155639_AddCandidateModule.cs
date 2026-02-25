using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaasKit.Migrations
{
    /// <inheritdoc />
    public partial class AddCandidateModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "client_communication_logs");

            migrationBuilder.DropTable(
                name: "client_documents");

            migrationBuilder.DropTable(
                name: "discount_cards");

            migrationBuilder.DropTable(
                name: "leads");

            migrationBuilder.DropTable(
                name: "nationality_pricing");

            migrationBuilder.DropTable(
                name: "worker_languages");

            migrationBuilder.DropTable(
                name: "worker_media");

            migrationBuilder.DropTable(
                name: "worker_passport_custody");

            migrationBuilder.DropTable(
                name: "worker_skills");

            migrationBuilder.DropTable(
                name: "worker_state_history");

            migrationBuilder.DropTable(
                name: "clients");

            migrationBuilder.DropTable(
                name: "workers");

            migrationBuilder.CreateTable(
                name: "candidates",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    full_name_en = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    full_name_ar = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    nationality = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    date_of_birth = table.Column<DateOnly>(type: "date", nullable: true),
                    gender = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    passport_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    source_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    tenant_supplier_id = table.Column<Guid>(type: "uuid", nullable: true),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    status_changed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    status_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    passport_expiry = table.Column<DateOnly>(type: "date", nullable: true),
                    medical_status = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    visa_status = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    expected_arrival_date = table.Column<DateOnly>(type: "date", nullable: true),
                    actual_arrival_date = table.Column<DateOnly>(type: "date", nullable: true),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    external_reference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
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
                    table.PrimaryKey("pk_candidates", x => x.id);
                    table.ForeignKey(
                        name: "fk_candidates_tenant_supplier_tenant_supplier_id",
                        column: x => x.tenant_supplier_id,
                        principalTable: "tenant_suppliers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "candidate_status_history",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    candidate_id = table.Column<Guid>(type: "uuid", nullable: false),
                    from_status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    to_status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    changed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    changed_by = table.Column<Guid>(type: "uuid", nullable: true),
                    reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_candidate_status_history", x => x.id);
                    table.ForeignKey(
                        name: "fk_candidate_status_history_candidate_candidate_id",
                        column: x => x.candidate_id,
                        principalTable: "candidates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_candidate_status_history_candidate_id",
                table: "candidate_status_history",
                column: "candidate_id");

            migrationBuilder.CreateIndex(
                name: "ix_candidate_status_history_changed_at",
                table: "candidate_status_history",
                column: "changed_at");

            migrationBuilder.CreateIndex(
                name: "ix_candidate_status_history_tenant_id",
                table: "candidate_status_history",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_candidates_created_at",
                table: "candidates",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_candidates_full_name_en",
                table: "candidates",
                column: "full_name_en");

            migrationBuilder.CreateIndex(
                name: "ix_candidates_nationality",
                table: "candidates",
                column: "nationality");

            migrationBuilder.CreateIndex(
                name: "ix_candidates_source_type",
                table: "candidates",
                column: "source_type");

            migrationBuilder.CreateIndex(
                name: "ix_candidates_status",
                table: "candidates",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_candidates_tenant_id",
                table: "candidates",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_candidates_tenant_id_passport_number",
                table: "candidates",
                columns: new[] { "tenant_id", "passport_number" },
                unique: true,
                filter: "passport_number IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_candidates_tenant_id_status",
                table: "candidates",
                columns: new[] { "tenant_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_candidates_tenant_supplier_id",
                table: "candidates",
                column: "tenant_supplier_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "candidate_status_history");

            migrationBuilder.DropTable(
                name: "candidates");

            migrationBuilder.CreateTable(
                name: "clients",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    blocked_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    category = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    emirate = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    emirates_id = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    full_name_ar = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    full_name_en = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    is_verified = table.Column<bool>(type: "boolean", nullable: false),
                    nationality = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    passport_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    sponsor_file_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    verified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    verified_by_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_clients", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "nationality_pricing",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    contract_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "AED"),
                    effective_from = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    effective_to = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    nationality = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_nationality_pricing", x => x.id);
                    table.CheckConstraint("ck_nationality_pricing_amount", "amount > 0");
                });

            migrationBuilder.CreateTable(
                name: "workers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    job_category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    current_status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    cv_serial = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    date_of_birth = table.Column<DateOnly>(type: "date", nullable: false),
                    education = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    emirates_id = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    full_name_ar = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    full_name_en = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    gender = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    is_available_for_flexible = table.Column<bool>(type: "boolean", nullable: false),
                    marital_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    monthly_base_salary = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    nationality = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    number_of_children = table.Column<int>(type: "integer", nullable: true),
                    passport_location = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    passport_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    photo_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    religion = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    video_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    years_of_experience = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_workers", x => x.id);
                    table.ForeignKey(
                        name: "fk_workers_job_categories_job_category_id",
                        column: x => x.job_category_id,
                        principalTable: "job_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "client_communication_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    client_id = table.Column<Guid>(type: "uuid", nullable: false),
                    channel = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    direction = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    logged_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    occurred_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    summary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_client_communication_logs", x => x.id);
                    table.ForeignKey(
                        name: "fk_client_communication_logs_client_client_id",
                        column: x => x.client_id,
                        principalTable: "clients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "client_documents",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    client_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    document_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    file_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    file_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    is_verified = table.Column<bool>(type: "boolean", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    uploaded_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    uploaded_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    verified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    verified_by_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_client_documents", x => x.id);
                    table.ForeignKey(
                        name: "fk_client_documents_clients_client_id",
                        column: x => x.client_id,
                        principalTable: "clients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "discount_cards",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    client_id = table.Column<Guid>(type: "uuid", nullable: false),
                    card_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    card_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    discount_percentage = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    valid_until = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_discount_cards", x => x.id);
                    table.CheckConstraint("ck_discount_cards_percentage", "discount_percentage >= 0 AND discount_percentage <= 100");
                    table.ForeignKey(
                        name: "fk_discount_cards_clients_client_id",
                        column: x => x.client_id,
                        principalTable: "clients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "leads",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    client_id = table.Column<Guid>(type: "uuid", nullable: true),
                    assigned_to_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    contact_email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    contact_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    contact_phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    converted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    source = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_leads", x => x.id);
                    table.ForeignKey(
                        name: "fk_leads_clients_client_id",
                        column: x => x.client_id,
                        principalTable: "clients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "worker_languages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    worker_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    language = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    proficiency = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_worker_languages", x => x.id);
                    table.ForeignKey(
                        name: "fk_worker_languages_workers_worker_id",
                        column: x => x.worker_id,
                        principalTable: "workers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "worker_media",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    worker_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    file_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    is_primary = table.Column<bool>(type: "boolean", nullable: false),
                    media_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    uploaded_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    uploaded_by_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_worker_media", x => x.id);
                    table.ForeignKey(
                        name: "fk_worker_media_worker_worker_id",
                        column: x => x.worker_id,
                        principalTable: "workers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "worker_passport_custody",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    worker_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    handed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    handed_to_entity_id = table.Column<Guid>(type: "uuid", nullable: true),
                    handed_to_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    location = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    received_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    recorded_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_worker_passport_custody", x => x.id);
                    table.ForeignKey(
                        name: "fk_worker_passport_custody_worker_worker_id",
                        column: x => x.worker_id,
                        principalTable: "workers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "worker_skills",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    worker_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    rating = table.Column<int>(type: "integer", nullable: false),
                    skill_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_worker_skills", x => x.id);
                    table.CheckConstraint("ck_worker_skills_rating", "rating >= 0 AND rating <= 100");
                    table.ForeignKey(
                        name: "fk_worker_skills_workers_worker_id",
                        column: x => x.worker_id,
                        principalTable: "workers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "worker_state_history",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    worker_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    from_status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    occurred_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    related_entity_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    to_status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    triggered_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_worker_state_history", x => x.id);
                    table.ForeignKey(
                        name: "fk_worker_state_history_worker_worker_id",
                        column: x => x.worker_id,
                        principalTable: "workers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_client_communication_logs_channel",
                table: "client_communication_logs",
                column: "channel");

            migrationBuilder.CreateIndex(
                name: "ix_client_communication_logs_client_occurred",
                table: "client_communication_logs",
                columns: new[] { "client_id", "occurred_at" });

            migrationBuilder.CreateIndex(
                name: "ix_client_documents_client_type",
                table: "client_documents",
                columns: new[] { "client_id", "document_type" });

            migrationBuilder.CreateIndex(
                name: "ix_client_documents_expires_at",
                table: "client_documents",
                column: "expires_at",
                filter: "expires_at IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_client_documents_is_verified",
                table: "client_documents",
                column: "is_verified");

            migrationBuilder.CreateIndex(
                name: "ix_clients_category",
                table: "clients",
                column: "category");

            migrationBuilder.CreateIndex(
                name: "ix_clients_created_at",
                table: "clients",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_clients_emirate",
                table: "clients",
                column: "emirate");

            migrationBuilder.CreateIndex(
                name: "ix_clients_full_name_ar",
                table: "clients",
                column: "full_name_ar");

            migrationBuilder.CreateIndex(
                name: "ix_clients_full_name_en",
                table: "clients",
                column: "full_name_en");

            migrationBuilder.CreateIndex(
                name: "ix_clients_is_verified",
                table: "clients",
                column: "is_verified");

            migrationBuilder.CreateIndex(
                name: "ix_clients_nationality",
                table: "clients",
                column: "nationality");

            migrationBuilder.CreateIndex(
                name: "ix_clients_sponsor_file_status",
                table: "clients",
                column: "sponsor_file_status");

            migrationBuilder.CreateIndex(
                name: "ix_clients_tenant_emirates_id",
                table: "clients",
                columns: new[] { "tenant_id", "emirates_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_discount_cards_card_type",
                table: "discount_cards",
                column: "card_type");

            migrationBuilder.CreateIndex(
                name: "ix_discount_cards_client_id",
                table: "discount_cards",
                column: "client_id");

            migrationBuilder.CreateIndex(
                name: "ix_discount_cards_type_number",
                table: "discount_cards",
                columns: new[] { "card_type", "card_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_leads_assigned_to",
                table: "leads",
                column: "assigned_to_user_id",
                filter: "assigned_to_user_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_leads_client_id",
                table: "leads",
                column: "client_id",
                filter: "client_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_leads_created_at",
                table: "leads",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_leads_source",
                table: "leads",
                column: "source");

            migrationBuilder.CreateIndex(
                name: "ix_leads_status",
                table: "leads",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_nationality_pricing_contract_type",
                table: "nationality_pricing",
                column: "contract_type");

            migrationBuilder.CreateIndex(
                name: "ix_nationality_pricing_effective_range",
                table: "nationality_pricing",
                columns: new[] { "effective_from", "effective_to" });

            migrationBuilder.CreateIndex(
                name: "ix_nationality_pricing_lookup",
                table: "nationality_pricing",
                columns: new[] { "tenant_id", "nationality", "contract_type", "effective_from" });

            migrationBuilder.CreateIndex(
                name: "ix_nationality_pricing_nationality",
                table: "nationality_pricing",
                column: "nationality");

            migrationBuilder.CreateIndex(
                name: "ix_worker_languages_worker_language",
                table: "worker_languages",
                columns: new[] { "worker_id", "language" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_worker_media_worker_primary",
                table: "worker_media",
                columns: new[] { "worker_id", "is_primary" },
                filter: "is_primary = true");

            migrationBuilder.CreateIndex(
                name: "ix_worker_media_worker_type",
                table: "worker_media",
                columns: new[] { "worker_id", "media_type" });

            migrationBuilder.CreateIndex(
                name: "ix_worker_passport_custody_handed_to_entity",
                table: "worker_passport_custody",
                column: "handed_to_entity_id",
                filter: "handed_to_entity_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_worker_passport_custody_worker_created",
                table: "worker_passport_custody",
                columns: new[] { "worker_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_worker_skills_worker_skill",
                table: "worker_skills",
                columns: new[] { "worker_id", "skill_name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_worker_state_history_related_entity",
                table: "worker_state_history",
                column: "related_entity_id",
                filter: "related_entity_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_worker_state_history_to_status",
                table: "worker_state_history",
                column: "to_status");

            migrationBuilder.CreateIndex(
                name: "ix_worker_state_history_worker_occurred",
                table: "worker_state_history",
                columns: new[] { "worker_id", "occurred_at" });

            migrationBuilder.CreateIndex(
                name: "ix_workers_created_at",
                table: "workers",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_workers_current_status",
                table: "workers",
                column: "current_status");

            migrationBuilder.CreateIndex(
                name: "ix_workers_full_name_ar",
                table: "workers",
                column: "full_name_ar");

            migrationBuilder.CreateIndex(
                name: "ix_workers_full_name_en",
                table: "workers",
                column: "full_name_en");

            migrationBuilder.CreateIndex(
                name: "ix_workers_is_available_flexible",
                table: "workers",
                column: "is_available_for_flexible");

            migrationBuilder.CreateIndex(
                name: "ix_workers_job_category_id",
                table: "workers",
                column: "job_category_id");

            migrationBuilder.CreateIndex(
                name: "ix_workers_nationality",
                table: "workers",
                column: "nationality");

            migrationBuilder.CreateIndex(
                name: "ix_workers_passport_location",
                table: "workers",
                column: "passport_location");

            migrationBuilder.CreateIndex(
                name: "ix_workers_tenant_cv_serial",
                table: "workers",
                columns: new[] { "tenant_id", "cv_serial" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_workers_tenant_passport",
                table: "workers",
                columns: new[] { "tenant_id", "passport_number" },
                unique: true);
        }
    }
}
