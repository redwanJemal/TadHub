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
            // Tenancy module
            new() { Id = Guid.NewGuid(), Name = "tenancy.view", Description = "View tenant details", Module = "tenancy", DisplayOrder = 1 },
            new() { Id = Guid.NewGuid(), Name = "tenancy.manage", Description = "Manage tenant settings", Module = "tenancy", DisplayOrder = 2 },
            new() { Id = Guid.NewGuid(), Name = "tenancy.delete", Description = "Delete tenant", Module = "tenancy", DisplayOrder = 3 },

            // Members module
            new() { Id = Guid.NewGuid(), Name = "members.view", Description = "View team members", Module = "members", DisplayOrder = 1 },
            new() { Id = Guid.NewGuid(), Name = "members.invite", Description = "Invite new members", Module = "members", DisplayOrder = 2 },
            new() { Id = Guid.NewGuid(), Name = "members.manage", Description = "Manage member roles", Module = "members", DisplayOrder = 3 },
            new() { Id = Guid.NewGuid(), Name = "members.remove", Description = "Remove members", Module = "members", DisplayOrder = 4 },

            // Roles module
            new() { Id = Guid.NewGuid(), Name = "roles.view", Description = "View roles", Module = "roles", DisplayOrder = 1 },
            new() { Id = Guid.NewGuid(), Name = "roles.manage", Description = "Create and edit roles", Module = "roles", DisplayOrder = 2 },
            new() { Id = Guid.NewGuid(), Name = "roles.delete", Description = "Delete roles", Module = "roles", DisplayOrder = 3 },
            new() { Id = Guid.NewGuid(), Name = "roles.assign", Description = "Assign roles to users", Module = "roles", DisplayOrder = 4 },

            // Billing module
            new() { Id = Guid.NewGuid(), Name = "billing.view", Description = "View billing information", Module = "billing", DisplayOrder = 1 },
            new() { Id = Guid.NewGuid(), Name = "billing.manage", Description = "Manage subscription and payments", Module = "billing", DisplayOrder = 2 },

            // API module
            new() { Id = Guid.NewGuid(), Name = "api.view", Description = "View API keys", Module = "api", DisplayOrder = 1 },
            new() { Id = Guid.NewGuid(), Name = "api.manage", Description = "Create and revoke API keys", Module = "api", DisplayOrder = 2 },

            // Settings module
            new() { Id = Guid.NewGuid(), Name = "settings.view", Description = "View settings", Module = "settings", DisplayOrder = 1 },
            new() { Id = Guid.NewGuid(), Name = "settings.manage", Description = "Manage settings", Module = "settings", DisplayOrder = 2 },

            // Portal module
            new() { Id = Guid.NewGuid(), Name = "portal.view", Description = "View portals", Module = "portal", DisplayOrder = 1 },
            new() { Id = Guid.NewGuid(), Name = "portal.manage", Description = "Create and manage portals", Module = "portal", DisplayOrder = 2 },
            new() { Id = Guid.NewGuid(), Name = "portal.delete", Description = "Delete portals", Module = "portal", DisplayOrder = 3 },

            // Content module
            new() { Id = Guid.NewGuid(), Name = "content.view", Description = "View content", Module = "content", DisplayOrder = 1 },
            new() { Id = Guid.NewGuid(), Name = "content.create", Description = "Create content", Module = "content", DisplayOrder = 2 },
            new() { Id = Guid.NewGuid(), Name = "content.edit", Description = "Edit content", Module = "content", DisplayOrder = 3 },
            new() { Id = Guid.NewGuid(), Name = "content.delete", Description = "Delete content", Module = "content", DisplayOrder = 4 },
            new() { Id = Guid.NewGuid(), Name = "content.publish", Description = "Publish content", Module = "content", DisplayOrder = 5 },

            // Analytics module
            new() { Id = Guid.NewGuid(), Name = "analytics.view", Description = "View analytics", Module = "analytics", DisplayOrder = 1 },
            new() { Id = Guid.NewGuid(), Name = "analytics.export", Description = "Export analytics data", Module = "analytics", DisplayOrder = 2 },

            // Notifications module
            new() { Id = Guid.NewGuid(), Name = "notifications.view", Description = "View notifications", Module = "notifications", DisplayOrder = 1 },
            new() { Id = Guid.NewGuid(), Name = "notifications.send", Description = "Send notifications to users", Module = "notifications", DisplayOrder = 2 },
        };
    }
}
