using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaasKit.Migrations
{
    /// <inheritdoc />
    public partial class IdentityTenancyAuthorizationRefactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_tenants_tenant_type_tenant_type_id",
                table: "tenants");

            migrationBuilder.DropForeignKey(
                name: "fk_tenants_tenants_parent_tenant_id",
                table: "tenants");

            migrationBuilder.DropTable(
                name: "admin_users");

            migrationBuilder.DropTable(
                name: "group_roles");

            migrationBuilder.DropTable(
                name: "group_users");

            migrationBuilder.DropTable(
                name: "tenant_type_relationships");

            migrationBuilder.DropTable(
                name: "tenant_users");

            migrationBuilder.DropTable(
                name: "groups");

            migrationBuilder.DropTable(
                name: "tenant_types");

            migrationBuilder.DropIndex(
                name: "ix_tenants_parent_tenant_id",
                table: "tenants");

            migrationBuilder.DropIndex(
                name: "ix_tenants_tenant_type_id",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "parent_tenant_id",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "tenant_type_id",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "role",
                table: "tenant_user_invitations");

            migrationBuilder.AddColumn<string>(
                name: "name_ar",
                table: "tenants",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "default_role_id",
                table: "tenant_user_invitations",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_custom",
                table: "roles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "template_id",
                table: "roles",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "scope",
                table: "permissions",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "platform_staff",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "admin"),
                    department = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_platform_staff", x => x.id);
                    table.ForeignKey(
                        name: "fk_platform_staff_user_profile_user_id",
                        column: x => x.user_id,
                        principalTable: "user_profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "role_templates",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_system = table.Column<bool>(type: "boolean", nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_role_templates", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "suppliers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name_en = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    name_ar = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    country = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    license_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    website = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_suppliers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tenant_memberships",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_owner = table.Column<bool>(type: "boolean", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    joined_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tenant_memberships", x => x.id);
                    table.ForeignKey(
                        name: "fk_tenant_memberships_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_tenant_memberships_user_profiles_user_id",
                        column: x => x.user_id,
                        principalTable: "user_profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "role_template_permissions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    template_id = table.Column<Guid>(type: "uuid", nullable: false),
                    permission_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_role_template_permissions", x => x.id);
                    table.ForeignKey(
                        name: "fk_role_template_permissions_permissions_permission_id",
                        column: x => x.permission_id,
                        principalTable: "permissions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_role_template_permissions_role_templates_template_id",
                        column: x => x.template_id,
                        principalTable: "role_templates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "supplier_contacts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    supplier_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    full_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    job_title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_primary = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_supplier_contacts", x => x.id);
                    table.ForeignKey(
                        name: "fk_supplier_contacts_suppliers_supplier_id",
                        column: x => x.supplier_id,
                        principalTable: "suppliers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_supplier_contacts_user_profiles_user_id",
                        column: x => x.user_id,
                        principalTable: "user_profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "tenant_suppliers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    supplier_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    contract_reference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    agreement_start_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    agreement_end_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tenant_suppliers", x => x.id);
                    table.ForeignKey(
                        name: "fk_tenant_suppliers_suppliers_supplier_id",
                        column: x => x.supplier_id,
                        principalTable: "suppliers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_roles_template_id",
                table: "roles",
                column: "template_id");

            migrationBuilder.CreateIndex(
                name: "ix_platform_staff_user_id",
                table: "platform_staff",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_role_template_permissions_permission_id",
                table: "role_template_permissions",
                column: "permission_id");

            migrationBuilder.CreateIndex(
                name: "ix_role_template_permissions_template_permission",
                table: "role_template_permissions",
                columns: new[] { "template_id", "permission_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_role_templates_name",
                table: "role_templates",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_supplier_contacts_is_active",
                table: "supplier_contacts",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_supplier_contacts_supplier_id",
                table: "supplier_contacts",
                column: "supplier_id");

            migrationBuilder.CreateIndex(
                name: "ix_supplier_contacts_supplier_id_email",
                table: "supplier_contacts",
                columns: new[] { "supplier_id", "email" },
                unique: true,
                filter: "email IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_supplier_contacts_user_id",
                table: "supplier_contacts",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_suppliers_country",
                table: "suppliers",
                column: "country");

            migrationBuilder.CreateIndex(
                name: "ix_suppliers_created_at",
                table: "suppliers",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_suppliers_is_active",
                table: "suppliers",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_suppliers_license_number",
                table: "suppliers",
                column: "license_number",
                unique: true,
                filter: "license_number IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_suppliers_name_en",
                table: "suppliers",
                column: "name_en");

            migrationBuilder.CreateIndex(
                name: "ix_tenant_memberships_is_owner",
                table: "tenant_memberships",
                column: "is_owner");

            migrationBuilder.CreateIndex(
                name: "ix_tenant_memberships_tenant_id",
                table: "tenant_memberships",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_tenant_memberships_tenant_user",
                table: "tenant_memberships",
                columns: new[] { "tenant_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tenant_memberships_user_id",
                table: "tenant_memberships",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_tenant_suppliers_created_at",
                table: "tenant_suppliers",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_tenant_suppliers_status",
                table: "tenant_suppliers",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_tenant_suppliers_supplier_id",
                table: "tenant_suppliers",
                column: "supplier_id");

            migrationBuilder.CreateIndex(
                name: "ix_tenant_suppliers_tenant_id",
                table: "tenant_suppliers",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_tenant_suppliers_tenant_id_supplier_id",
                table: "tenant_suppliers",
                columns: new[] { "tenant_id", "supplier_id" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_roles_role_template_template_id",
                table: "roles",
                column: "template_id",
                principalTable: "role_templates",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_roles_role_template_template_id",
                table: "roles");

            migrationBuilder.DropTable(
                name: "platform_staff");

            migrationBuilder.DropTable(
                name: "role_template_permissions");

            migrationBuilder.DropTable(
                name: "supplier_contacts");

            migrationBuilder.DropTable(
                name: "tenant_memberships");

            migrationBuilder.DropTable(
                name: "tenant_suppliers");

            migrationBuilder.DropTable(
                name: "role_templates");

            migrationBuilder.DropTable(
                name: "suppliers");

            migrationBuilder.DropIndex(
                name: "ix_roles_template_id",
                table: "roles");

            migrationBuilder.DropColumn(
                name: "name_ar",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "default_role_id",
                table: "tenant_user_invitations");

            migrationBuilder.DropColumn(
                name: "is_custom",
                table: "roles");

            migrationBuilder.DropColumn(
                name: "template_id",
                table: "roles");

            migrationBuilder.DropColumn(
                name: "scope",
                table: "permissions");

            migrationBuilder.AddColumn<Guid>(
                name: "parent_tenant_id",
                table: "tenants",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "tenant_type_id",
                table: "tenants",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "role",
                table: "tenant_user_invitations",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "admin_users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    is_super_admin = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_admin_users", x => x.id);
                    table.ForeignKey(
                        name: "fk_admin_users_user_profile_user_id",
                        column: x => x.user_id,
                        principalTable: "user_profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "groups",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_groups", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tenant_types",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tenant_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tenant_users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    joined_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tenant_users", x => x.id);
                    table.ForeignKey(
                        name: "fk_tenant_users_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_tenant_users_user_profiles_user_id",
                        column: x => x.user_id,
                        principalTable: "user_profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "group_roles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    group_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_group_roles", x => x.id);
                    table.ForeignKey(
                        name: "fk_group_roles_groups_group_id",
                        column: x => x.group_id,
                        principalTable: "groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_group_roles_role_role_id",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "group_users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    group_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    joined_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_group_users", x => x.id);
                    table.ForeignKey(
                        name: "fk_group_users_groups_group_id",
                        column: x => x.group_id,
                        principalTable: "groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_group_users_user_profiles_user_id",
                        column: x => x.user_id,
                        principalTable: "user_profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tenant_type_relationships",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    child_type_id = table.Column<Guid>(type: "uuid", nullable: false),
                    parent_type_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tenant_type_relationships", x => x.id);
                    table.ForeignKey(
                        name: "fk_tenant_type_relationships_tenant_types_child_type_id",
                        column: x => x.child_type_id,
                        principalTable: "tenant_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_tenant_type_relationships_tenant_types_parent_type_id",
                        column: x => x.parent_type_id,
                        principalTable: "tenant_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_tenants_parent_tenant_id",
                table: "tenants",
                column: "parent_tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_tenants_tenant_type_id",
                table: "tenants",
                column: "tenant_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_admin_users_user_id",
                table: "admin_users",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_group_roles_group_role",
                table: "group_roles",
                columns: new[] { "group_id", "role_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_group_roles_role_id",
                table: "group_roles",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "ix_group_users_group_user",
                table: "group_users",
                columns: new[] { "group_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_group_users_user_id",
                table: "group_users",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_groups_tenant_id",
                table: "groups",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_groups_tenant_name",
                table: "groups",
                columns: new[] { "tenant_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tenant_type_relationships_child_type_id",
                table: "tenant_type_relationships",
                column: "child_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_tenant_type_relationships_parent_child",
                table: "tenant_type_relationships",
                columns: new[] { "parent_type_id", "child_type_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tenant_types_display_order",
                table: "tenant_types",
                column: "display_order");

            migrationBuilder.CreateIndex(
                name: "ix_tenant_types_name",
                table: "tenant_types",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tenant_users_role",
                table: "tenant_users",
                column: "role");

            migrationBuilder.CreateIndex(
                name: "ix_tenant_users_tenant_id",
                table: "tenant_users",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_tenant_users_tenant_user",
                table: "tenant_users",
                columns: new[] { "tenant_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tenant_users_user_id",
                table: "tenant_users",
                column: "user_id");

            migrationBuilder.AddForeignKey(
                name: "fk_tenants_tenant_type_tenant_type_id",
                table: "tenants",
                column: "tenant_type_id",
                principalTable: "tenant_types",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_tenants_tenants_parent_tenant_id",
                table: "tenants",
                column: "parent_tenant_id",
                principalTable: "tenants",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
