using Contract.Contracts.DTOs;

namespace TadHub.Api.Documents;

public sealed record ContractPdfData(
    ContractDto Contract,
    string TenantName,
    string? TenantNameAr,
    byte[]? TenantLogo);
