using Identity.Core.Entities;
using TadHub.SharedKernel.Entities;

namespace Authorization.Core.Entities;

/// <summary>
/// Membership of a user in a group.
/// </summary>
public class GroupUser : BaseEntity
{
    /// <summary>
    /// The group ID.
    /// </summary>
    public Guid GroupId { get; set; }

    /// <summary>
    /// Navigation property for the group.
    /// </summary>
    public Group Group { get; set; } = null!;

    /// <summary>
    /// The user ID.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Navigation property for the user.
    /// </summary>
    public UserProfile User { get; set; } = null!;

    /// <summary>
    /// When the user joined the group.
    /// </summary>
    public DateTimeOffset JoinedAt { get; set; }
}
