using Authorization.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TadHub.Infrastructure.Persistence;

namespace Authorization.Core.Seeds;

/// <summary>
/// Seeds default permissions on application startup.
/// </summary>
public class PermissionSeeder : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PermissionSeeder> _logger;

    public PermissionSeeder(IServiceProvider serviceProvider, ILogger<PermissionSeeder> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await SeedPermissionsAsync(db, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task SeedPermissionsAsync(AppDbContext db, CancellationToken ct)
    {
        var permissions = GetDefaultPermissions();

        foreach (var permission in permissions)
        {
            var exists = await db.Set<Permission>()
                .AnyAsync(p => p.Name == permission.Name, ct);

            if (!exists)
            {
                db.Set<Permission>().Add(permission);
                _logger.LogInformation("Seeding permission: {Permission}", permission.Name);
            }
        }

        await db.SaveChangesAsync(ct);
        _logger.LogInformation("Permission seeding completed");
    }

    private static List<Permission> GetDefaultPermissions()
    {
        return new List<Permission>
        {
            // Tenancy module (Both — visible at platform and tenant level)
            new() { Id = Guid.NewGuid(), Name = "tenancy.view", Description = "View tenant details", Module = "tenancy", Scope = PermissionScope.Both, DisplayOrder = 1 },
            new() { Id = Guid.NewGuid(), Name = "tenancy.manage", Description = "Manage tenant settings", Module = "tenancy", Scope = PermissionScope.Both, DisplayOrder = 2 },
            new() { Id = Guid.NewGuid(), Name = "tenancy.delete", Description = "Delete tenant", Module = "tenancy", Scope = PermissionScope.Both, DisplayOrder = 3 },

            // Members module (Tenant)
            new() { Id = Guid.NewGuid(), Name = "members.view", Description = "View team members", Module = "members", Scope = PermissionScope.Tenant, DisplayOrder = 1 },
            new() { Id = Guid.NewGuid(), Name = "members.invite", Description = "Invite new members", Module = "members", Scope = PermissionScope.Tenant, DisplayOrder = 2 },
            new() { Id = Guid.NewGuid(), Name = "members.manage", Description = "Manage member roles", Module = "members", Scope = PermissionScope.Tenant, DisplayOrder = 3 },
            new() { Id = Guid.NewGuid(), Name = "members.remove", Description = "Remove members", Module = "members", Scope = PermissionScope.Tenant, DisplayOrder = 4 },

            // Roles module (Tenant)
            new() { Id = Guid.NewGuid(), Name = "roles.view", Description = "View roles", Module = "roles", Scope = PermissionScope.Tenant, DisplayOrder = 1 },
            new() { Id = Guid.NewGuid(), Name = "roles.manage", Description = "Create and edit roles", Module = "roles", Scope = PermissionScope.Tenant, DisplayOrder = 2 },
            new() { Id = Guid.NewGuid(), Name = "roles.delete", Description = "Delete roles", Module = "roles", Scope = PermissionScope.Tenant, DisplayOrder = 3 },
            new() { Id = Guid.NewGuid(), Name = "roles.assign", Description = "Assign roles to users", Module = "roles", Scope = PermissionScope.Tenant, DisplayOrder = 4 },

            // Billing module (Tenant)
            new() { Id = Guid.NewGuid(), Name = "billing.view", Description = "View billing information", Module = "billing", Scope = PermissionScope.Tenant, DisplayOrder = 1 },
            new() { Id = Guid.NewGuid(), Name = "billing.manage", Description = "Manage subscription and payments", Module = "billing", Scope = PermissionScope.Tenant, DisplayOrder = 2 },

            // API module (Tenant)
            new() { Id = Guid.NewGuid(), Name = "api.view", Description = "View API keys", Module = "api", Scope = PermissionScope.Tenant, DisplayOrder = 1 },
            new() { Id = Guid.NewGuid(), Name = "api.manage", Description = "Create and revoke API keys", Module = "api", Scope = PermissionScope.Tenant, DisplayOrder = 2 },

            // Settings module (Tenant)
            new() { Id = Guid.NewGuid(), Name = "settings.view", Description = "View settings", Module = "settings", Scope = PermissionScope.Tenant, DisplayOrder = 1 },
            new() { Id = Guid.NewGuid(), Name = "settings.manage", Description = "Manage settings", Module = "settings", Scope = PermissionScope.Tenant, DisplayOrder = 2 },

            // Portal module (Tenant)
            new() { Id = Guid.NewGuid(), Name = "portal.view", Description = "View portals", Module = "portal", Scope = PermissionScope.Tenant, DisplayOrder = 1 },
            new() { Id = Guid.NewGuid(), Name = "portal.manage", Description = "Create and manage portals", Module = "portal", Scope = PermissionScope.Tenant, DisplayOrder = 2 },
            new() { Id = Guid.NewGuid(), Name = "portal.delete", Description = "Delete portals", Module = "portal", Scope = PermissionScope.Tenant, DisplayOrder = 3 },

            // Content module (Tenant)
            new() { Id = Guid.NewGuid(), Name = "content.view", Description = "View content", Module = "content", Scope = PermissionScope.Tenant, DisplayOrder = 1 },
            new() { Id = Guid.NewGuid(), Name = "content.create", Description = "Create content", Module = "content", Scope = PermissionScope.Tenant, DisplayOrder = 2 },
            new() { Id = Guid.NewGuid(), Name = "content.edit", Description = "Edit content", Module = "content", Scope = PermissionScope.Tenant, DisplayOrder = 3 },
            new() { Id = Guid.NewGuid(), Name = "content.delete", Description = "Delete content", Module = "content", Scope = PermissionScope.Tenant, DisplayOrder = 4 },
            new() { Id = Guid.NewGuid(), Name = "content.publish", Description = "Publish content", Module = "content", Scope = PermissionScope.Tenant, DisplayOrder = 5 },

            // Analytics module (Both)
            new() { Id = Guid.NewGuid(), Name = "analytics.view", Description = "View analytics", Module = "analytics", Scope = PermissionScope.Both, DisplayOrder = 1 },
            new() { Id = Guid.NewGuid(), Name = "analytics.export", Description = "Export analytics data", Module = "analytics", Scope = PermissionScope.Both, DisplayOrder = 2 },

            // Notifications module (Tenant)
            new() { Id = Guid.NewGuid(), Name = "notifications.view", Description = "View notifications", Module = "notifications", Scope = PermissionScope.Tenant, DisplayOrder = 1 },
            new() { Id = Guid.NewGuid(), Name = "notifications.send", Description = "Send notifications to users", Module = "notifications", Scope = PermissionScope.Tenant, DisplayOrder = 2 },

            // ── New domain permissions ──

            // Platform-scoped permissions
            new() { Id = Guid.NewGuid(), Name = "tenants.create", Description = "Create new tenants", Module = "platform", Scope = PermissionScope.Platform, DisplayOrder = 1 },
            new() { Id = Guid.NewGuid(), Name = "tenants.suspend", Description = "Suspend tenants", Module = "platform", Scope = PermissionScope.Platform, DisplayOrder = 2 },
            new() { Id = Guid.NewGuid(), Name = "platform.users.manage", Description = "Manage platform users", Module = "platform", Scope = PermissionScope.Platform, DisplayOrder = 3 },
            new() { Id = Guid.NewGuid(), Name = "platform.billing.view", Description = "View platform billing", Module = "platform", Scope = PermissionScope.Platform, DisplayOrder = 4 },

            // Client Management module (Tenant)
            new() { Id = Guid.NewGuid(), Name = "clients.view", Description = "View clients", Module = "clients", Scope = PermissionScope.Tenant, DisplayOrder = 1 },
            new() { Id = Guid.NewGuid(), Name = "clients.create", Description = "Create clients", Module = "clients", Scope = PermissionScope.Tenant, DisplayOrder = 2 },
            new() { Id = Guid.NewGuid(), Name = "clients.edit", Description = "Edit clients", Module = "clients", Scope = PermissionScope.Tenant, DisplayOrder = 3 },
            new() { Id = Guid.NewGuid(), Name = "clients.delete", Description = "Delete clients", Module = "clients", Scope = PermissionScope.Tenant, DisplayOrder = 4 },

            // Worker Management module (Tenant)
            new() { Id = Guid.NewGuid(), Name = "workers.view", Description = "View workers", Module = "workers", Scope = PermissionScope.Tenant, DisplayOrder = 1 },
            new() { Id = Guid.NewGuid(), Name = "workers.create", Description = "Create workers", Module = "workers", Scope = PermissionScope.Tenant, DisplayOrder = 2 },
            new() { Id = Guid.NewGuid(), Name = "workers.edit", Description = "Edit workers", Module = "workers", Scope = PermissionScope.Tenant, DisplayOrder = 3 },
            new() { Id = Guid.NewGuid(), Name = "workers.manage_status", Description = "Transition worker status", Module = "workers", Scope = PermissionScope.Tenant, DisplayOrder = 4 },
            new() { Id = Guid.NewGuid(), Name = "workers.delete", Description = "Delete workers", Module = "workers", Scope = PermissionScope.Tenant, DisplayOrder = 5 },

            // Contracts module (Tenant)
            new() { Id = Guid.NewGuid(), Name = "contracts.view", Description = "View contracts", Module = "contracts", Scope = PermissionScope.Tenant, DisplayOrder = 1 },
            new() { Id = Guid.NewGuid(), Name = "contracts.create", Description = "Create contracts", Module = "contracts", Scope = PermissionScope.Tenant, DisplayOrder = 2 },
            new() { Id = Guid.NewGuid(), Name = "contracts.edit", Description = "Edit contracts", Module = "contracts", Scope = PermissionScope.Tenant, DisplayOrder = 3 },
            new() { Id = Guid.NewGuid(), Name = "contracts.manage_status", Description = "Transition contract status", Module = "contracts", Scope = PermissionScope.Tenant, DisplayOrder = 4 },
            new() { Id = Guid.NewGuid(), Name = "contracts.delete", Description = "Delete contracts", Module = "contracts", Scope = PermissionScope.Tenant, DisplayOrder = 5 },

            // Documents module (Tenant)
            new() { Id = Guid.NewGuid(), Name = "documents.view", Description = "View worker documents", Module = "documents", Scope = PermissionScope.Tenant, DisplayOrder = 1 },
            new() { Id = Guid.NewGuid(), Name = "documents.create", Description = "Add worker documents", Module = "documents", Scope = PermissionScope.Tenant, DisplayOrder = 2 },
            new() { Id = Guid.NewGuid(), Name = "documents.edit", Description = "Edit worker documents", Module = "documents", Scope = PermissionScope.Tenant, DisplayOrder = 3 },
            new() { Id = Guid.NewGuid(), Name = "documents.delete", Description = "Delete worker documents", Module = "documents", Scope = PermissionScope.Tenant, DisplayOrder = 4 },

            // Suppliers module (Tenant)
            new() { Id = Guid.NewGuid(), Name = "suppliers.view", Description = "View suppliers", Module = "suppliers", Scope = PermissionScope.Tenant, DisplayOrder = 1 },
            new() { Id = Guid.NewGuid(), Name = "suppliers.manage", Description = "Manage suppliers", Module = "suppliers", Scope = PermissionScope.Tenant, DisplayOrder = 2 },
            new() { Id = Guid.NewGuid(), Name = "suppliers.delete", Description = "Delete suppliers", Module = "suppliers", Scope = PermissionScope.Tenant, DisplayOrder = 3 },

            // Finance module (Tenant)
            new() { Id = Guid.NewGuid(), Name = "finance.view", Description = "View financial data", Module = "finance", Scope = PermissionScope.Tenant, DisplayOrder = 1 },
            new() { Id = Guid.NewGuid(), Name = "finance.manage", Description = "Manage financial records", Module = "finance", Scope = PermissionScope.Tenant, DisplayOrder = 2 },
            new() { Id = Guid.NewGuid(), Name = "finance.approve", Description = "Approve financial transactions", Module = "finance", Scope = PermissionScope.Tenant, DisplayOrder = 3 },

            // Candidates module (Tenant)
            new() { Id = Guid.NewGuid(), Name = "candidates.view", Description = "View candidates", Module = "candidates", Scope = PermissionScope.Tenant, DisplayOrder = 1 },
            new() { Id = Guid.NewGuid(), Name = "candidates.create", Description = "Create candidates", Module = "candidates", Scope = PermissionScope.Tenant, DisplayOrder = 2 },
            new() { Id = Guid.NewGuid(), Name = "candidates.edit", Description = "Edit candidate profiles", Module = "candidates", Scope = PermissionScope.Tenant, DisplayOrder = 3 },
            new() { Id = Guid.NewGuid(), Name = "candidates.manage_status", Description = "Transition candidate status", Module = "candidates", Scope = PermissionScope.Tenant, DisplayOrder = 4 },
            new() { Id = Guid.NewGuid(), Name = "candidates.delete", Description = "Delete candidates", Module = "candidates", Scope = PermissionScope.Tenant, DisplayOrder = 5 },

            // Audit module (Tenant)
            new() { Id = Guid.NewGuid(), Name = "audit.view", Description = "View audit logs", Module = "audit", Scope = PermissionScope.Tenant, DisplayOrder = 1 },
            new() { Id = Guid.NewGuid(), Name = "audit.export", Description = "Export audit data", Module = "audit", Scope = PermissionScope.Tenant, DisplayOrder = 2 },
        };
    }
}
