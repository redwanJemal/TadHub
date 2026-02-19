using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using SaasKit.SharedKernel.Interfaces;

namespace SaasKit.Infrastructure.Persistence;

/// <summary>
/// Factory for creating AppDbContext at design time for EF Core migrations.
/// Used by `dotnet ef migrations add` and `dotnet ef database update`.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        // Build configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=saaskit_dev;Username=saaskit;Password=saaskit";

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(connectionString, npgsqlOptions =>
        {
            npgsqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
            npgsqlOptions.MigrationsHistoryTable("__ef_migrations_history");
        });

        return new AppDbContext(optionsBuilder.Options, new DesignTimeTenantContext());
    }

    /// <summary>
    /// Stub tenant context for design-time operations.
    /// </summary>
    private sealed class DesignTimeTenantContext : ITenantContext
    {
        public Guid TenantId => Guid.Empty;
        public string TenantSlug => "design-time";
        public bool IsResolved => false;
    }
}
