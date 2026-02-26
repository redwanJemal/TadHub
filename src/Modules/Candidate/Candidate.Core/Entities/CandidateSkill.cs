using TadHub.SharedKernel.Entities;

namespace Candidate.Core.Entities;

public enum SkillProficiency
{
    Basic,
    Intermediate,
    Advanced,
    Expert,
}

/// <summary>
/// A skill entry for a candidate with proficiency rating.
/// </summary>
public class CandidateSkill : TenantScopedEntity
{
    public Guid CandidateId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public SkillProficiency ProficiencyLevel { get; set; }

    // Navigation
    public Candidate Candidate { get; set; } = null!;
}
