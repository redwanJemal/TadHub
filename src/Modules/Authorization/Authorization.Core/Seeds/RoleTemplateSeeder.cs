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

            // 1. Sync template: add any missing permissions
            var templatePermIds = template.Permissions.Select(p => p.PermissionId).ToHashSet();
            var newTemplatePerms = expectedPermissions.Where(p => !templatePermIds.Contains(p.Id)).ToList();

            foreach (var perm in newTemplatePerms)
            {
                db.Set<RoleTemplatePermission>().Add(new RoleTemplatePermission
                {
                    Id = Guid.NewGuid(),
                    TemplateId = template.Id,
                    PermissionId = perm.Id
                });
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

                foreach (var perm in missingPerms)
                {
                    db.Set<RolePermission>().Add(new RolePermission
                    {
                        Id = Guid.NewGuid(),
                        RoleId = role.Id,
                        PermissionId = perm.Id
                    });
                }

                if (missingPerms.Count > 0)
                {
                    _logger.LogInformation("Synced {Count} permissions into role '{Role}' (tenant {Tenant})",
                        missingPerms.Count, role.Name, role.TenantId);
                    changed = true;
                }
            }

            if (newTemplatePerms.Count > 0) changed = true;
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

            // Accountant: billing, view, and financial permissions
            new()
            {
                Name = "Accountant",
                Description = "Financial and billing access.",
                IsSystem = false,
                DisplayOrder = 3,
                PermissionFilter = p =>
                    p.Module == "billing" ||
                    p.Name.EndsWith(".view") ||
                    p.Module == "analytics"
            },

            // Sales: customer-facing, content, and portal permissions
            new()
            {
                Name = "Sales",
                Description = "Sales and customer-facing access.",
                IsSystem = false,
                DisplayOrder = 4,
                PermissionFilter = p =>
                    p.Name.EndsWith(".view") ||
                    p.Module == "content" ||
                    p.Module == "portal" ||
                    p.Module == "notifications"
            },

            // Operations: manage most resources, no billing or role management
            new()
            {
                Name = "Operations",
                Description = "Operational access for day-to-day management.",
                IsSystem = false,
                DisplayOrder = 5,
                PermissionFilter = p =>
                    p.Module != "billing" &&
                    p.Module != "roles" &&
                    !p.Name.EndsWith(".delete")
            },

            // Viewer: read-only access
            new()
            {
                Name = "Viewer",
                Description = "Read-only access to all features.",
                IsSystem = false,
                DisplayOrder = 6,
                PermissionFilter = p => p.Name.EndsWith(".view")
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
