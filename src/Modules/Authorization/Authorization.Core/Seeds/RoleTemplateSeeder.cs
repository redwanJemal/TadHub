using Authorization.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TadHub.Infrastructure.Persistence;

namespace Authorization.Core.Seeds;

/// <summary>
/// Seeds default role templates on application startup.
/// Role templates define the starting set of roles cloned into each new tenant.
/// </summary>
public class RoleTemplateSeeder : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RoleTemplateSeeder> _logger;

    public RoleTemplateSeeder(IServiceProvider serviceProvider, ILogger<RoleTemplateSeeder> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await SeedTemplatesAsync(db, cancellationToken);
        await SyncNewPermissionsAsync(db, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task SeedTemplatesAsync(AppDbContext db, CancellationToken ct)
    {
        // Load all existing permissions (tenant-scoped and both)
        var allPermissions = await db.Set<Permission>()
            .Where(p => p.Scope == PermissionScope.Tenant || p.Scope == PermissionScope.Both)
            .ToListAsync(ct);

        if (allPermissions.Count == 0)
        {
            _logger.LogWarning("No tenant-scoped permissions found. Skipping role template seeding.");
            return;
        }

        var templateDefinitions = GetTemplateDefinitions();

        foreach (var def in templateDefinitions)
        {
            var existingTemplate = await db.Set<RoleTemplate>()
                .Include(t => t.Permissions)
                .FirstOrDefaultAsync(t => t.Name == def.Name, ct);

            if (existingTemplate is not null)
            {
                _logger.LogDebug("Role template '{Name}' already exists, skipping", def.Name);
                continue;
            }

            var template = new RoleTemplate
            {
                Id = Guid.NewGuid(),
                Name = def.Name,
                Description = def.Description,
                IsSystem = def.IsSystem,
                DisplayOrder = def.DisplayOrder
            };

            db.Set<RoleTemplate>().Add(template);

            // Resolve permissions for this template
            var templatePermissions = ResolvePermissions(allPermissions, def.PermissionFilter);

            foreach (var permission in templatePermissions)
            {
                db.Set<RoleTemplatePermission>().Add(new RoleTemplatePermission
                {
                    Id = Guid.NewGuid(),
                    TemplateId = template.Id,
                    PermissionId = permission.Id
                });
            }

            _logger.LogInformation(
                "Seeded role template '{Name}' with {Count} permissions",
                def.Name, templatePermissions.Count);
        }

        await db.SaveChangesAsync(ct);
        _logger.LogInformation("Role template seeding completed");
    }

    /// <summary>
    /// Syncs newly-added permissions into existing role templates AND propagates
    /// them to all tenant roles derived from those templates.
    /// Runs every startup — idempotent via duplicate checks.
    /// </summary>
    private async Task SyncNewPermissionsAsync(AppDbContext db, CancellationToken ct)
    {
        var allPermissions = await db.Set<Permission>()
            .Where(p => p.Scope == PermissionScope.Tenant || p.Scope == PermissionScope.Both)
            .ToListAsync(ct);

        if (allPermissions.Count == 0) return;

        var templateDefinitions = GetTemplateDefinitions();
        var changed = false;

        foreach (var def in templateDefinitions)
        {
            var template = await db.Set<RoleTemplate>()
                .Include(t => t.Permissions)
                .FirstOrDefaultAsync(t => t.Name == def.Name, ct);

            if (template is null) continue;

            var expectedPermissions = ResolvePermissions(allPermissions, def.PermissionFilter);
            var expectedPermIds = expectedPermissions.Select(p => p.Id).ToHashSet();

            // 1. Sync template: add missing and remove extra permissions
            var templatePermIds = template.Permissions.Select(p => p.PermissionId).ToHashSet();
            var newTemplatePerms = expectedPermissions.Where(p => !templatePermIds.Contains(p.Id)).ToList();
            var extraTemplatePerms = template.Permissions.Where(p => !expectedPermIds.Contains(p.PermissionId)).ToList();

            foreach (var perm in newTemplatePerms)
            {
                db.Set<RoleTemplatePermission>().Add(new RoleTemplatePermission
                {
                    Id = Guid.NewGuid(),
                    TemplateId = template.Id,
                    PermissionId = perm.Id
                });
            }

            if (extraTemplatePerms.Count > 0)
            {
                db.Set<RoleTemplatePermission>().RemoveRange(extraTemplatePerms);
                _logger.LogInformation("Removed {Count} permissions from template '{Template}'",
                    extraTemplatePerms.Count, template.Name);
            }

            if (newTemplatePerms.Count > 0)
            {
                _logger.LogInformation("Added {Count} new permissions to template '{Template}'",
                    newTemplatePerms.Count, template.Name);
            }

            // 2. Sync tenant roles: match by TemplateId or by Name (legacy roles)
            var tenantRoles = await db.Set<Role>()
                .IgnoreQueryFilters()
                .Include(r => r.Permissions)
                .Where(r => r.TemplateId == template.Id
                    || (r.TemplateId == null && r.Name == template.Name))
                .ToListAsync(ct);

            foreach (var role in tenantRoles)
            {
                var rolePermIds = role.Permissions.Select(rp => rp.PermissionId).ToHashSet();
                var missingPerms = expectedPermissions.Where(p => !rolePermIds.Contains(p.Id)).ToList();
                var extraPerms = role.Permissions.Where(rp => !expectedPermIds.Contains(rp.PermissionId)).ToList();

                foreach (var perm in missingPerms)
                {
                    db.Set<RolePermission>().Add(new RolePermission
                    {
                        Id = Guid.NewGuid(),
                        RoleId = role.Id,
                        PermissionId = perm.Id
                    });
                }

                if (extraPerms.Count > 0)
                {
                    db.Set<RolePermission>().RemoveRange(extraPerms);
                    _logger.LogInformation("Removed {Count} extra permissions from role '{Role}' (tenant {Tenant})",
                        extraPerms.Count, role.Name, role.TenantId);
                    changed = true;
                }

                if (missingPerms.Count > 0)
                {
                    _logger.LogInformation("Synced {Count} permissions into role '{Role}' (tenant {Tenant})",
                        missingPerms.Count, role.Name, role.TenantId);
                    changed = true;
                }
            }

            if (newTemplatePerms.Count > 0 || extraTemplatePerms.Count > 0) changed = true;
        }

        if (changed)
        {
            await db.SaveChangesAsync(ct);
            _logger.LogInformation("Permission sync completed");
        }
    }

    /// <summary>
    /// Resolves which permissions to include based on the filter function.
    /// </summary>
    private static List<Permission> ResolvePermissions(
        List<Permission> allPermissions,
        Func<Permission, bool> filter)
    {
        return allPermissions.Where(filter).ToList();
    }

    /// <summary>
    /// Returns the template definitions with their permission filters.
    /// </summary>
    private static List<TemplateDefinition> GetTemplateDefinitions()
    {
        return new List<TemplateDefinition>
        {
            // Owner: all permissions
            new()
            {
                Name = "Owner",
                Description = "Full access to all features. Assigned to the tenant creator.",
                IsSystem = true,
                DisplayOrder = 1,
                PermissionFilter = _ => true
            },

            // Admin: all permissions except delete operations
            new()
            {
                Name = "Admin",
                Description = "Administrative access. All permissions except destructive operations.",
                IsSystem = true,
                DisplayOrder = 2,
                PermissionFilter = p => !p.Name.EndsWith(".delete") && p.Name != "tenancy.delete"
            },

            // Accountant: finance, clients, contracts, workers (view), dashboard, packages (view)
            new()
            {
                Name = "Accountant",
                Description = "Financial and billing access with read-only view of clients, contracts, and workers.",
                IsSystem = false,
                DisplayOrder = 3,
                PermissionFilter = p =>
                    p.Module == "financial" ||
                    p.Module == "billing" ||
                    p.Module == "analytics" ||
                    p.Name == "dashboard.view" ||
                    p.Name == "clients.view" ||
                    p.Name == "contracts.view" ||
                    p.Name == "workers.view" ||
                    p.Name == "packages.view" ||
                    p.Name == "reports.view" ||
                    p.Name == "reports.export" ||
                    p.Name == "notifications.view"
            },

            // Sales: suppliers, candidates, workers, clients, placements, trials, contracts
            // Note: Sales can create/edit but NOT transition statuses or delete —
            // status transitions (e.g. candidate approval) require Admin authority,
            // delete operations are reserved for Owner only
            new()
            {
                Name = "Sales",
                Description = "Sales access for recruitment, placements, and client management.",
                IsSystem = false,
                DisplayOrder = 4,
                PermissionFilter = p =>
                    (p.Module == "suppliers" ||
                     p.Module == "candidates" ||
                     p.Module == "workers" ||
                     p.Module == "clients" ||
                     p.Module == "placements" ||
                     p.Module == "trials" ||
                     p.Module == "contracts" ||
                     p.Module == "content" ||
                     p.Name == "notifications.view" ||
                     p.Name == "dashboard.view" ||
                     p.Name == "reports.view" ||
                     p.Name == "reports.export")
                    && !p.Name.EndsWith(".manage_status")
                    && !p.Name.EndsWith(".delete")
            },

            // Operations: arrivals, accommodations, visas, compliance, workers (view/edit/status), returnees, runaways
            // Note: Operations cannot delete or create workers (workers come from candidate approval).
            // Notifications limited to view-only. Delete operations reserved for Owner.
            new()
            {
                Name = "Operations",
                Description = "Operational access for arrivals, accommodations, visas, and case management.",
                IsSystem = false,
                DisplayOrder = 5,
                PermissionFilter = p =>
                    (p.Module == "arrivals" ||
                     p.Module == "accommodations" ||
                     p.Module == "visas" ||
                     p.Module == "documents" ||
                     p.Module == "returnees" ||
                     p.Module == "runaways" ||
                     p.Name == "workers.view" ||
                     p.Name == "workers.edit" ||
                     p.Name == "workers.manage_status" ||
                     p.Name == "notifications.view" ||
                     p.Name == "dashboard.view" ||
                     p.Name == "clients.view" ||
                     p.Name == "contracts.view" ||
                     p.Name == "placements.view" ||
                     p.Name == "suppliers.view" ||
                     p.Name == "candidates.view" ||
                     p.Name == "reports.view" ||
                     p.Name == "reports.export")
                    && !p.Name.EndsWith(".delete")
            },

            // Viewer: dashboard + read-only access to core operational data
            new()
            {
                Name = "Viewer",
                Description = "Read-only access to dashboard and basic operational data.",
                IsSystem = false,
                DisplayOrder = 6,
                PermissionFilter = p =>
                    p.Name == "dashboard.view" ||
                    p.Name == "workers.view" ||
                    p.Name == "clients.view" ||
                    p.Name == "contracts.view" ||
                    p.Name == "placements.view" ||
                    p.Name == "arrivals.view" ||
                    p.Name == "reports.view" ||
                    p.Name == "notifications.view"
            },

            // Driver: limited access to assigned pickups and dashboard landing page
            new()
            {
                Name = "Driver",
                Description = "Driver role with access to assigned pickups, confirm pickup, and upload photos.",
                IsSystem = false,
                DisplayOrder = 7,
                PermissionFilter = p =>
                    p.Name == "dashboard.view" ||
                    p.Name == "arrivals.driver_actions"
            },

            // Accommodation Staff: limited to accommodation management and viewing arrivals
            new()
            {
                Name = "Accommodation Staff",
                Description = "Accommodation staff with access to check-in/check-out, occupant lists, and incoming arrivals.",
                IsSystem = false,
                DisplayOrder = 8,
                PermissionFilter = p =>
                    p.Name == "dashboard.view" ||
                    p.Name == "accommodations.view" ||
                    p.Name == "accommodations.manage" ||
                    p.Name == "arrivals.view"
            }
        };
    }

    /// <summary>
    /// Internal definition for template seeding.
    /// </summary>
    private sealed class TemplateDefinition
    {
        public string Name { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public bool IsSystem { get; init; }
        public int DisplayOrder { get; init; }
        public Func<Permission, bool> PermissionFilter { get; init; } = _ => false;
    }
}
