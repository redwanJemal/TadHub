using System.Text.Json.Serialization;

namespace Financial.Contracts.Settings;

public sealed class TenantFinancialSettings
{
    // VAT
    [JsonPropertyName("vatRate")]
    public decimal VatRate { get; set; } = 5.00m;

    [JsonPropertyName("vatEnabled")]
    public bool VatEnabled { get; set; } = true;

    [JsonPropertyName("taxRegistrationNumber")]
    public string? TaxRegistrationNumber { get; set; }

    // Currency & numbering
    [JsonPropertyName("defaultCurrency")]
    public string DefaultCurrency { get; set; } = "AED";

    [JsonPropertyName("invoicePrefix")]
    public string InvoicePrefix { get; set; } = "INV";

    [JsonPropertyName("paymentPrefix")]
    public string PaymentPrefix { get; set; } = "PAY";

    [JsonPropertyName("invoiceDueDays")]
    public int InvoiceDueDays { get; set; } = 30;

    // Payment milestones
    [JsonPropertyName("requireDepositOnBooking")]
    public bool RequireDepositOnBooking { get; set; } = true;

    [JsonPropertyName("depositPercentage")]
    public decimal DepositPercentage { get; set; } = 50.00m;

    [JsonPropertyName("enableInstallments")]
    public bool EnableInstallments { get; set; } = false;

    [JsonPropertyName("maxInstallments")]
    public int MaxInstallments { get; set; } = 3;

    // Enabled payment methods
    [JsonPropertyName("paymentMethods")]
    public List<string> PaymentMethods { get; set; } = ["Cash", "Card", "BankTransfer", "Cheque", "EDirham"];

    // Invoice branding
    [JsonPropertyName("invoiceFooterText")]
    public string? InvoiceFooterText { get; set; }

    [JsonPropertyName("invoiceFooterTextAr")]
    public string? InvoiceFooterTextAr { get; set; }

    [JsonPropertyName("invoiceTerms")]
    public string? InvoiceTerms { get; set; }

    [JsonPropertyName("invoiceTermsAr")]
    public string? InvoiceTermsAr { get; set; }

    // Auto-generation
    [JsonPropertyName("autoGenerateInvoiceOnConfirm")]
    public bool AutoGenerateInvoiceOnConfirm { get; set; } = true;
}
