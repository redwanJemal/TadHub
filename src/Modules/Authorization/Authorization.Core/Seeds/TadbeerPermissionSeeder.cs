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
            new() { Id = Guid.NewGuid(), Name = "clients.register", Description = "Register new clients", Module = "clients", DisplayOrder = 1 },
            new() { Id = Guid.NewGuid(), Name = "clients.verify", Description = "Verify client documents", Module = "clients", DisplayOrder = 2 },
            new() { Id = Guid.NewGuid(), Name = "clients.manage", Description = "Manage client records", Module = "clients", DisplayOrder = 3 },

            // Worker/CV Management module
            new() { Id = Guid.NewGuid(), Name = "workers.manage", Description = "Manage worker records", Module = "workers", DisplayOrder = 1 },
            new() { Id = Guid.NewGuid(), Name = "workers.cv.edit", Description = "Edit worker CV/profile", Module = "workers", DisplayOrder = 2 },
            new() { Id = Guid.NewGuid(), Name = "workers.search", Description = "Search available workers", Module = "workers", DisplayOrder = 3 },
            new() { Id = Guid.NewGuid(), Name = "workers.passport.custody", Description = "Manage passport custody", Module = "workers", DisplayOrder = 4 },

            // Contract Engine module
            new() { Id = Guid.NewGuid(), Name = "contracts.create", Description = "Create new contracts", Module = "contracts", DisplayOrder = 1 },
            new() { Id = Guid.NewGuid(), Name = "contracts.approve", Description = "Approve contracts", Module = "contracts", DisplayOrder = 2 },
            new() { Id = Guid.NewGuid(), Name = "contracts.terminate", Description = "Terminate contracts", Module = "contracts", DisplayOrder = 3 },
            new() { Id = Guid.NewGuid(), Name = "contracts.refund", Description = "Process contract refunds", Module = "contracts", DisplayOrder = 4 },

            // Financial module
            new() { Id = Guid.NewGuid(), Name = "financial.payments.process", Description = "Process payments", Module = "financial", DisplayOrder = 1 },
            new() { Id = Guid.NewGuid(), Name = "financial.invoices.generate", Description = "Generate invoices", Module = "financial", DisplayOrder = 2 },
            new() { Id = Guid.NewGuid(), Name = "financial.xreport.generate", Description = "Generate X-Reports (daily cash)", Module = "financial", DisplayOrder = 3 },
            new() { Id = Guid.NewGuid(), Name = "financial.refunds.process", Description = "Process refunds", Module = "financial", DisplayOrder = 4 },

            // PRO/Government Gateway module
            new() { Id = Guid.NewGuid(), Name = "pro.tasks.manage", Description = "Manage PRO tasks", Module = "pro", DisplayOrder = 1 },
            new() { Id = Guid.NewGuid(), Name = "pro.visa.apply", Description = "Apply for visas", Module = "pro", DisplayOrder = 2 },
            new() { Id = Guid.NewGuid(), Name = "pro.documents.manage", Description = "Manage government documents", Module = "pro", DisplayOrder = 3 },

            // Scheduling module
            new() { Id = Guid.NewGuid(), Name = "scheduling.bookings.create", Description = "Create bookings", Module = "scheduling", DisplayOrder = 1 },
            new() { Id = Guid.NewGuid(), Name = "scheduling.bookings.cancel", Description = "Cancel bookings", Module = "scheduling", DisplayOrder = 2 },

            // WPS module
            new() { Id = Guid.NewGuid(), Name = "wps.payroll.manage", Description = "Manage payroll records", Module = "wps", DisplayOrder = 1 },
            new() { Id = Guid.NewGuid(), Name = "wps.sif.submit", Description = "Submit SIF files to bank", Module = "wps", DisplayOrder = 2 },

            // Reports module
            new() { Id = Guid.NewGuid(), Name = "reports.view", Description = "View reports", Module = "reports", DisplayOrder = 1 },
            new() { Id = Guid.NewGuid(), Name = "reports.export", Description = "Export reports", Module = "reports", DisplayOrder = 2 },
            new() { Id = Guid.NewGuid(), Name = "reports.mohre", Description = "Generate MoHRE compliance reports", Module = "reports", DisplayOrder = 3 },
        };
    }
}
