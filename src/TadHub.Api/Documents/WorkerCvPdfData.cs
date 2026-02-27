using Worker.Contracts.DTOs;

namespace TadHub.Api.Documents;

public sealed record WorkerCvPdfData(
    WorkerCvDto Cv,
    string TenantName,
    string? TenantNameAr,
    byte[]? TenantLogo,
    byte[]? WorkerPhoto);
