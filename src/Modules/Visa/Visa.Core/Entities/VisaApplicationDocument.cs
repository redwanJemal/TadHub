using TadHub.SharedKernel.Entities;

namespace Visa.Core.Entities;

public class VisaApplicationDocument : TenantScopedEntity
{
    public Guid VisaApplicationId { get; set; }
    public VisaDocumentType DocumentType { get; set; }
    public string FileUrl { get; set; } = string.Empty;
    public DateTimeOffset UploadedAt { get; set; }
    public bool IsVerified { get; set; }

    // Navigation
    public VisaApplication VisaApplication { get; set; } = null!;
}
