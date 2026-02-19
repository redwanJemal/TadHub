using TadHub.SharedKernel.Entities;

namespace Worker.Core.Entities;

/// <summary>
/// Worker language proficiency.
/// </summary>
public class WorkerLanguage : TenantScopedEntity
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
    /// Language name (English, Arabic, Tagalog, etc.).
    /// </summary>
    public string Language { get; set; } = string.Empty;

    /// <summary>
    /// Proficiency level.
    /// </summary>
    public LanguageProficiency Proficiency { get; set; }
}
