using TadHub.SharedKernel.Entities;

namespace Worker.Core.Entities;

/// <summary>
/// Worker skill with proficiency rating.
/// </summary>
public class WorkerSkill : TenantScopedEntity
{
    /// <summary>
    /// Worker ID FK.
    /// </summary>
    public Guid WorkerId { get; set; }

    /// <summary>
    /// Worker navigation.
    /// </summary>
    public Worker? Worker { get; set; }

    /// <summary>
    /// Skill name (Cooking, Cleaning, Childcare, etc.).
    /// </summary>
    public string SkillName { get; set; } = string.Empty;

    /// <summary>
    /// Rating 0-100.
    /// </summary>
    public int Rating { get; set; }
}
