using TadHub.SharedKernel.Entities;

namespace Candidate.Core.Entities;

public enum LanguageProficiency
{
    Basic,
    Conversational,
    Fluent,
    Native,
}

/// <summary>
/// A language entry for a candidate with proficiency rating.
/// </summary>
public class CandidateLanguage : TenantScopedEntity
{
    public Guid CandidateId { get; set; }
    public string Language { get; set; } = string.Empty;
    public LanguageProficiency ProficiencyLevel { get; set; }

    // Navigation
    public Candidate Candidate { get; set; } = null!;
}
