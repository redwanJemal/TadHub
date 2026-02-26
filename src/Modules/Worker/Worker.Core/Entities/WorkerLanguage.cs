using TadHub.SharedKernel.Entities;

namespace Worker.Core.Entities;

public class WorkerLanguage : TenantScopedEntity
{
    public Guid WorkerId { get; set; }
    public string Language { get; set; } = string.Empty;
    public string ProficiencyLevel { get; set; } = string.Empty;

    // Navigation
    public Worker Worker { get; set; } = null!;
}
