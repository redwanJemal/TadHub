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

            // Invoices module (Tenant)
            new() { Id = Guid.NewGuid(), Name = "invoices.view", Description = "View invoices", Module = "financial", Scope = PermissionScope.Tenant, DisplayOrder = 1 },
            new() { Id = Guid.NewGuid(), Name = "invoices.create", Description = "Create invoices", Module = "financial", Scope = PermissionScope.Tenant, DisplayOrder = 2 },
            new() { Id = Guid.NewGuid(), Name = "invoices.edit", Description = "Edit invoices", Module = "financial", Scope = PermissionScope.Tenant, DisplayOrder = 3 },
            new() { Id = Guid.NewGuid(), Name = "invoices.manage_status", Description = "Transition invoice status", Module = "financial", Scope = PermissionScope.Tenant, DisplayOrder = 4 },
            new() { Id = Guid.NewGuid(), Name = "invoices.delete", Description = "Delete invoices", Module = "financial", Scope = PermissionScope.Tenant, DisplayOrder = 5 },

            // Payments module (Tenant)
            new() { Id = Guid.NewGuid(), Name = "payments.view", Description = "View payments", Module = "financial", Scope = PermissionScope.Tenant, DisplayOrder = 6 },
            new() { Id = Guid.NewGuid(), Name = "payments.create", Description = "Record payments", Module = "financial", Scope = PermissionScope.Tenant, DisplayOrder = 7 },
            new() { Id = Guid.NewGuid(), Name = "payments.refund", Description = "Process payment refunds", Module = "financial", Scope = PermissionScope.Tenant, DisplayOrder = 8 },
            new() { Id = Guid.NewGuid(), Name = "payments.delete", Description = "Delete payments", Module = "financial", Scope = PermissionScope.Tenant, DisplayOrder = 9 },

            // Discount Programs module (Tenant)
            new() { Id = Guid.NewGuid(), Name = "discounts.view", Description = "View discount programs", Module = "financial", Scope = PermissionScope.Tenant, DisplayOrder = 10 },
            new() { Id = Guid.NewGuid(), Name = "discounts.create", Description = "Create discount programs", Module = "financial", Scope = PermissionScope.Tenant, DisplayOrder = 11 },
            new() { Id = Guid.NewGuid(), Name = "discounts.edit", Description = "Edit discount programs", Module = "financial", Scope = PermissionScope.Tenant, DisplayOrder = 12 },
            new() { Id = Guid.NewGuid(), Name = "discounts.delete", Description = "Delete discount programs", Module = "financial", Scope = PermissionScope.Tenant, DisplayOrder = 13 },

            // Supplier Payments module (Tenant)
            new() { Id = Guid.NewGuid(), Name = "supplier_payments.view", Description = "View supplier payments", Module = "financial", Scope = PermissionScope.Tenant, DisplayOrder = 14 },
            new() { Id = Guid.NewGuid(), Name = "supplier_payments.create", Description = "Create supplier payments", Module = "financial", Scope = PermissionScope.Tenant, DisplayOrder = 15 },
            new() { Id = Guid.NewGuid(), Name = "supplier_payments.edit", Description = "Edit supplier payments", Module = "financial", Scope = PermissionScope.Tenant, DisplayOrder = 16 },
            new() { Id = Guid.NewGuid(), Name = "supplier_payments.manage_status", Description = "Transition supplier payment status", Module = "financial", Scope = PermissionScope.Tenant, DisplayOrder = 17 },
            new() { Id = Guid.NewGuid(), Name = "supplier_payments.delete", Description = "Delete supplier payments", Module = "financial", Scope = PermissionScope.Tenant, DisplayOrder = 18 },

            // Financial Reports module (Tenant)
            new() { Id = Guid.NewGuid(), Name = "financial_reports.view", Description = "View financial reports", Module = "financial", Scope = PermissionScope.Tenant, DisplayOrder = 19 },
            new() { Id = Guid.NewGuid(), Name = "financial_reports.manage", Description = "Manage financial report settings", Module = "financial", Scope = PermissionScope.Tenant, DisplayOrder = 20 },

            // Candidates module (Tenant)
            new() { Id = Guid.NewGuid(), Name = "candidates.view", Description = "View candidates", Module = "candidates", Scope = PermissionScope.Tenant, DisplayOrder = 1 },
            new() { Id = Guid.NewGuid(), Name = "candidates.create", Description = "Create candidates", Module = "candidates", Scope = PermissionScope.Tenant, DisplayOrder = 2 },
            new() { Id = Guid.NewGuid(), Name = "candidates.edit", Description = "Edit candidate profiles", Module = "candidates", Scope = PermissionScope.Tenant, DisplayOrder = 3 },
            new() { Id = Guid.NewGuid(), Name = "candidates.manage_status", Description = "Transition candidate status", Module = "candidates", Scope = PermissionScope.Tenant, DisplayOrder = 4 },
            new() { Id = Guid.NewGuid(), Name = "candidates.delete", Description = "Delete candidates", Module = "candidates", Scope = PermissionScope.Tenant, DisplayOrder = 5 },

            // Audit module (Tenant)
            new() { Id = Guid.NewGuid(), Name = "audit.view", Description = "View audit logs", Module = "audit", Scope = PermissionScope.Tenant, DisplayOrder = 1 },
            new() { Id = Guid.NewGuid(), Name = "audit.export", Description = "Export audit data", Module = "audit", Scope = PermissionScope.Tenant, DisplayOrder = 2 },

            // Dashboard module (Tenant)
            new() { Id = Guid.NewGuid(), Name = "dashboard.view", Description = "View dashboard", Module = "dashboard", Scope = PermissionScope.Tenant, DisplayOrder = 1 },
        };
    }
}
