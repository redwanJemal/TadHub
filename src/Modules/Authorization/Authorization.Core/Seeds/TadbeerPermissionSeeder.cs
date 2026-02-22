using Authorization.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TadHub.Infrastructure.Persistence;

namespace Authorization.Core.Seeds;

/// <summary>
/// Seeds Tadbeer domain permissions on application startup.
/// These permissions are for the domestic worker recruitment ERP modules.
/// </summary>
public class TadbeerPermissionSeeder : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TadbeerPermissionSeeder> _logger;

    public TadbeerPermissionSeeder(IServiceProvider serviceProvider, ILogger<TadbeerPermissionSeeder> logger)
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
        var permissions = GetTadbeerPermissions();

        foreach (var permission in permissions)
        {
            var exists = await db.Set<Permission>()
                .AnyAsync(p => p.Name == permission.Name, ct);

            if (!exists)
            {
                db.Set<Permission>().Add(permission);
                _logger.LogInformation("Seeding Tadbeer permission: {Permission}", permission.Name);
            }
        }

        await db.SaveChangesAsync(ct);
        _logger.LogInformation("Tadbeer permission seeding completed");
    }

    /// <summary>
    /// Returns all Tadbeer domain permissions.
    /// </summary>
    public static List<Permission> GetTadbeerPermissions()
    {
        return new List<Permission>
        {
            // Client Management module
            new() { Id = Guid.NewGuid(), Name = "clients.register", Description = "Register new clients", Module = "clients", Scope = PermissionScope.Tenant, DisplayOrder = 1 },
            new() { Id = Guid.NewGuid(), Name = "clients.verify", Description = "Verify client documents", Module = "clients", Scope = PermissionScope.Tenant, DisplayOrder = 2 },
            new() { Id = Guid.NewGuid(), Name = "clients.manage", Description = "Manage client records", Module = "clients", Scope = PermissionScope.Tenant, DisplayOrder = 3 },

            // Worker/CV Management module
            new() { Id = Guid.NewGuid(), Name = "workers.view", Description = "View worker records", Module = "workers", Scope = PermissionScope.Tenant, DisplayOrder = 1 },
            new() { Id = Guid.NewGuid(), Name = "workers.create", Description = "Create new workers", Module = "workers", Scope = PermissionScope.Tenant, DisplayOrder = 2 },
            new() { Id = Guid.NewGuid(), Name = "workers.update", Description = "Update worker records", Module = "workers", Scope = PermissionScope.Tenant, DisplayOrder = 3 },
            new() { Id = Guid.NewGuid(), Name = "workers.manage", Description = "Full worker management (delete, state changes)", Module = "workers", Scope = PermissionScope.Tenant, DisplayOrder = 4 },
            new() { Id = Guid.NewGuid(), Name = "workers.cv.edit", Description = "Edit worker CV/profile", Module = "workers", Scope = PermissionScope.Tenant, DisplayOrder = 5 },
            new() { Id = Guid.NewGuid(), Name = "workers.search", Description = "Search available workers", Module = "workers", Scope = PermissionScope.Tenant, DisplayOrder = 6 },
            new() { Id = Guid.NewGuid(), Name = "workers.passport.view", Description = "View passport information", Module = "workers", Scope = PermissionScope.Tenant, DisplayOrder = 7 },
            new() { Id = Guid.NewGuid(), Name = "workers.passport.manage", Description = "Manage passport custody", Module = "workers", Scope = PermissionScope.Tenant, DisplayOrder = 8 },

            // Contract Engine module
            new() { Id = Guid.NewGuid(), Name = "contracts.create", Description = "Create new contracts", Module = "contracts", Scope = PermissionScope.Tenant, DisplayOrder = 1 },
            new() { Id = Guid.NewGuid(), Name = "contracts.approve", Description = "Approve contracts", Module = "contracts", Scope = PermissionScope.Tenant, DisplayOrder = 2 },
            new() { Id = Guid.NewGuid(), Name = "contracts.terminate", Description = "Terminate contracts", Module = "contracts", Scope = PermissionScope.Tenant, DisplayOrder = 3 },
            new() { Id = Guid.NewGuid(), Name = "contracts.refund", Description = "Process contract refunds", Module = "contracts", Scope = PermissionScope.Tenant, DisplayOrder = 4 },

            // Financial module
            new() { Id = Guid.NewGuid(), Name = "financial.payments.process", Description = "Process payments", Module = "financial", Scope = PermissionScope.Tenant, DisplayOrder = 1 },
            new() { Id = Guid.NewGuid(), Name = "financial.invoices.generate", Description = "Generate invoices", Module = "financial", Scope = PermissionScope.Tenant, DisplayOrder = 2 },
            new() { Id = Guid.NewGuid(), Name = "financial.xreport.generate", Description = "Generate X-Reports (daily cash)", Module = "financial", Scope = PermissionScope.Tenant, DisplayOrder = 3 },
            new() { Id = Guid.NewGuid(), Name = "financial.refunds.process", Description = "Process refunds", Module = "financial", Scope = PermissionScope.Tenant, DisplayOrder = 4 },

            // PRO/Government Gateway module
            new() { Id = Guid.NewGuid(), Name = "pro.tasks.manage", Description = "Manage PRO tasks", Module = "pro", Scope = PermissionScope.Tenant, DisplayOrder = 1 },
            new() { Id = Guid.NewGuid(), Name = "pro.visa.apply", Description = "Apply for visas", Module = "pro", Scope = PermissionScope.Tenant, DisplayOrder = 2 },
            new() { Id = Guid.NewGuid(), Name = "pro.documents.manage", Description = "Manage government documents", Module = "pro", Scope = PermissionScope.Tenant, DisplayOrder = 3 },

            // Scheduling module
            new() { Id = Guid.NewGuid(), Name = "scheduling.bookings.create", Description = "Create bookings", Module = "scheduling", Scope = PermissionScope.Tenant, DisplayOrder = 1 },
            new() { Id = Guid.NewGuid(), Name = "scheduling.bookings.cancel", Description = "Cancel bookings", Module = "scheduling", Scope = PermissionScope.Tenant, DisplayOrder = 2 },

            // WPS module
            new() { Id = Guid.NewGuid(), Name = "wps.payroll.manage", Description = "Manage payroll records", Module = "wps", Scope = PermissionScope.Tenant, DisplayOrder = 1 },
            new() { Id = Guid.NewGuid(), Name = "wps.sif.submit", Description = "Submit SIF files to bank", Module = "wps", Scope = PermissionScope.Tenant, DisplayOrder = 2 },

            // Reports module
            new() { Id = Guid.NewGuid(), Name = "reports.view", Description = "View reports", Module = "reports", Scope = PermissionScope.Tenant, DisplayOrder = 1 },
            new() { Id = Guid.NewGuid(), Name = "reports.export", Description = "Export reports", Module = "reports", Scope = PermissionScope.Tenant, DisplayOrder = 2 },
            new() { Id = Guid.NewGuid(), Name = "reports.mohre", Description = "Generate MoHRE compliance reports", Module = "reports", Scope = PermissionScope.Tenant, DisplayOrder = 3 },
        };
    }
}
