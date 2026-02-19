using Authorization.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TadHub.Infrastructure.Persistence;

namespace Authorization.Core.Seeds;

/// <summary>
/// Seeds Tadbeer domain roles for a tenant.
/// Called when a new tenant is created to set up agency-specific roles.
/// </summary>
public class TadbeerRoleSeeder
{
    private readonly ILogger<TadbeerRoleSeeder> _logger;

    public TadbeerRoleSeeder(ILogger<TadbeerRoleSeeder> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Seeds the 7 Tadbeer domain roles for a tenant.
    /// </summary>
    public async Task SeedRolesAsync(AppDbContext db, Guid tenantId, CancellationToken ct)
    {
        var roles = GetTadbeerRoles(tenantId);

        foreach (var role in roles)
        {
            var exists = await db.Set<Role>()
                .IgnoreQueryFilters()
                .AnyAsync(r => r.TenantId == tenantId && r.Name == role.Name, ct);

            if (!exists)
            {
                db.Set<Role>().Add(role);
                _logger.LogInformation("Seeding Tadbeer role {Role} for tenant {TenantId}", role.Name, tenantId);
            }
        }

        await db.SaveChangesAsync(ct);

        // Now assign permissions to roles
        await AssignPermissionsToRolesAsync(db, tenantId, ct);
    }

    private static List<Role> GetTadbeerRoles(Guid tenantId)
    {
        return new List<Role>
        {
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Name = "agency-admin",
                Description = "Full administrative access to all agency operations",
                IsSystem = true,
                DisplayOrder = 10
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Name = "receptionist",
                Description = "Front desk operations: client registration, worker search, contract creation",
                IsSystem = true,
                DisplayOrder = 20
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Name = "cashier",
                Description = "Financial operations: payments, invoices, X-Reports",
                IsSystem = true,
                DisplayOrder = 30
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Name = "pro-officer",
                Description = "Government relations: visa, medical, Emirates ID, passport custody",
                IsSystem = true,
                DisplayOrder = 40
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Name = "agent",
                Description = "Worker management: CV editing, scheduling, worker search",
                IsSystem = true,
                DisplayOrder = 50
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Name = "accountant",
                Description = "Financial and compliance: all financial operations, WPS, reports",
                IsSystem = true,
                DisplayOrder = 60
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Name = "viewer",
                Description = "Read-only access to reports",
                IsSystem = true,
                DisplayOrder = 70
            }
        };
    }

    private async Task AssignPermissionsToRolesAsync(AppDbContext db, Guid tenantId, CancellationToken ct)
    {
        // Get all Tadbeer permissions
        var permissionNames = TadbeerPermissionSeeder.GetTadbeerPermissions().Select(p => p.Name).ToList();
        var permissions = await db.Set<Permission>()
            .Where(p => permissionNames.Contains(p.Name))
            .ToDictionaryAsync(p => p.Name, p => p.Id, ct);

        // Get roles for this tenant
        var roles = await db.Set<Role>()
            .IgnoreQueryFilters()
            .Where(r => r.TenantId == tenantId)
            .ToDictionaryAsync(r => r.Name, r => r.Id, ct);

        // Define role-permission mappings
        var rolePermissions = new Dictionary<string, List<string>>
        {
            ["agency-admin"] = permissionNames, // All permissions
            
            ["receptionist"] = new()
            {
                "clients.register",
                "clients.manage",
                "workers.search",
                "contracts.create"
            },
            
            ["cashier"] = new()
            {
                "financial.payments.process",
                "financial.invoices.generate",
                "financial.xreport.generate"
            },
            
            ["pro-officer"] = new()
            {
                "pro.tasks.manage",
                "pro.visa.apply",
                "pro.documents.manage",
                "workers.passport.custody",
                "contracts.approve"
            },
            
            ["agent"] = new()
            {
                "workers.manage",
                "workers.cv.edit",
                "workers.search",
                "scheduling.bookings.create",
                "scheduling.bookings.cancel"
            },
            
            ["accountant"] = new()
            {
                "financial.payments.process",
                "financial.invoices.generate",
                "financial.xreport.generate",
                "financial.refunds.process",
                "wps.payroll.manage",
                "wps.sif.submit",
                "reports.view",
                "reports.export",
                "reports.mohre"
            },
            
            ["viewer"] = new()
            {
                "reports.view"
            }
        };

        // Create role-permission assignments
        foreach (var (roleName, permNames) in rolePermissions)
        {
            if (!roles.TryGetValue(roleName, out var roleId))
                continue;

            foreach (var permName in permNames)
            {
                if (!permissions.TryGetValue(permName, out var permId))
                    continue;

                var exists = await db.Set<RolePermission>()
                    .AnyAsync(rp => rp.RoleId == roleId && rp.PermissionId == permId, ct);

                if (!exists)
                {
                    db.Set<RolePermission>().Add(new RolePermission
                    {
                        Id = Guid.NewGuid(),
                        RoleId = roleId,
                        PermissionId = permId
                    });
                }
            }
        }

        await db.SaveChangesAsync(ct);
        _logger.LogInformation("Assigned Tadbeer permissions to roles for tenant {TenantId}", tenantId);
    }
}
