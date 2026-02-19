using ClientManagement.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClientManagement.Core.Persistence;

/// <summary>
/// EF Core configuration for ClientCommunicationLog entity.
/// </summary>
public class ClientCommunicationLogConfiguration : IEntityTypeConfiguration<ClientCommunicationLog>
{
    public void Configure(EntityTypeBuilder<ClientCommunicationLog> builder)
    {
        builder.ToTable("client_communication_logs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ClientId)
            .IsRequired();

        builder.Property(x => x.Channel)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.Direction)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.Summary)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(x => x.LoggedByUserId)
            .IsRequired();

        builder.Property(x => x.OccurredAt)
            .IsRequired();

        // Index for client communication history
        builder.HasIndex(x => new { x.ClientId, x.OccurredAt })
            .HasDatabaseName("ix_client_communication_logs_client_occurred");

        // Index for channel filtering
        builder.HasIndex(x => x.Channel)
            .HasDatabaseName("ix_client_communication_logs_channel");
    }
}
