using TadHub.SharedKernel.Entities;

namespace ClientManagement.Core.Entities;

/// <summary>
/// Log of communication with a client.
/// </summary>
public class ClientCommunicationLog : TenantScopedEntity
{
    /// <summary>
    /// Client this log belongs to.
    /// </summary>
    public Guid ClientId { get; set; }

    /// <summary>
    /// Navigation property for client.
    /// </summary>
    public Client? Client { get; set; }

    /// <summary>
    /// Communication channel used.
    /// </summary>
    public CommunicationChannel Channel { get; set; }

    /// <summary>
    /// Direction of communication.
    /// </summary>
    public CommunicationDirection Direction { get; set; }

    /// <summary>
    /// Summary of the communication.
    /// </summary>
    public string Summary { get; set; } = string.Empty;

    /// <summary>
    /// User who logged this communication.
    /// </summary>
    public Guid LoggedByUserId { get; set; }

    /// <summary>
    /// When the communication occurred.
    /// </summary>
    public DateTimeOffset OccurredAt { get; set; }
}
