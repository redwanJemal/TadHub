using Financial.Contracts.DTOs;
using Financial.Contracts.Settings;

namespace TadHub.Api.Documents;

public sealed record InvoicePdfData(
    InvoiceDto Invoice,
    string TenantName,
    string? TenantNameAr,
    string? TenantWebsite,
    byte[]? TenantLogo,
    InvoiceTemplateSettings Template,
    string? FooterText,
    string? FooterTextAr,
    string? Terms,
    string? TermsAr,
    string? ClientName,
    string? ClientNameAr,
    string? WorkerName,
    string? WorkerNameAr,
    string? WorkerCode);
