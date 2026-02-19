using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;
using TadHub.SharedKernel.Interfaces;

namespace TadHub.Infrastructure.Persistence.Interceptors;

/// <summary>
/// EF Core interceptor that sets PostgreSQL session variable for Row Level Security.
/// Sets `app.current_tenant_id` on connection open for RLS policies.
/// </summary>
public sealed class RlsInterceptor : DbConnectionInterceptor
{
    private readonly ITenantContext _tenantContext;

    public RlsInterceptor(ITenantContext tenantContext)
    {
        _tenantContext = tenantContext;
    }

    public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
    {
        SetTenantVariable(connection);
        base.ConnectionOpened(connection, eventData);
    }

    public override async Task ConnectionOpenedAsync(
        DbConnection connection,
        ConnectionEndEventData eventData,
        CancellationToken cancellationToken = default)
    {
        await SetTenantVariableAsync(connection, cancellationToken);
        await base.ConnectionOpenedAsync(connection, eventData, cancellationToken);
    }

    private void SetTenantVariable(DbConnection connection)
    {
        if (!_tenantContext.IsResolved)
            return;

        using var command = connection.CreateCommand();
        command.CommandText = $"SET app.current_tenant_id = '{_tenantContext.TenantId}'";
        command.ExecuteNonQuery();
    }

    private async Task SetTenantVariableAsync(DbConnection connection, CancellationToken cancellationToken)
    {
        if (!_tenantContext.IsResolved)
            return;

        await using var command = connection.CreateCommand();
        command.CommandText = $"SET app.current_tenant_id = '{_tenantContext.TenantId}'";
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
