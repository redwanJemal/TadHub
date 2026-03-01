using System.Text.Json.Serialization;

namespace Financial.Contracts.Settings;

public sealed class InvoiceTemplateSettings
{
    [JsonPropertyName("primaryColor")]
    public string PrimaryColor { get; set; } = "#1a365d";

    [JsonPropertyName("accentColor")]
    public string AccentColor { get; set; } = "#2b6cb0";

    [JsonPropertyName("showLogo")]
    public bool ShowLogo { get; set; } = true;

    [JsonPropertyName("showArabicText")]
    public bool ShowArabicText { get; set; } = true;

    [JsonPropertyName("companyAddress")]
    public string? CompanyAddress { get; set; }

    [JsonPropertyName("companyAddressAr")]
    public string? CompanyAddressAr { get; set; }
}
