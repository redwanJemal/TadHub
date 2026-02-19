using System.Text.Json.Serialization;

namespace TadHub.SharedKernel.Localization;

/// <summary>
/// Value type for bilingual text (English/Arabic).
/// Stored as JSONB in PostgreSQL.
/// Used for any user-facing text that needs bilingual support.
/// </summary>
public record LocalizedString
{
    /// <summary>
    /// English text.
    /// </summary>
    [JsonPropertyName("en")]
    public string En { get; init; } = string.Empty;

    /// <summary>
    /// Arabic text.
    /// </summary>
    [JsonPropertyName("ar")]
    public string Ar { get; init; } = string.Empty;

    /// <summary>
    /// Creates an empty localized string.
    /// </summary>
    public LocalizedString() { }

    /// <summary>
    /// Creates a localized string with both languages.
    /// </summary>
    public LocalizedString(string en, string ar)
    {
        En = en;
        Ar = ar;
    }

    /// <summary>
    /// Creates a localized string with only English (Arabic same as English).
    /// </summary>
    public static LocalizedString FromEnglish(string en) => new(en, en);

    /// <summary>
    /// Creates a localized string with only Arabic (English same as Arabic).
    /// </summary>
    public static LocalizedString FromArabic(string ar) => new(ar, ar);

    /// <summary>
    /// Gets the text for the specified locale.
    /// Falls back to English if locale not found.
    /// </summary>
    public string Resolve(string? locale = null)
    {
        return locale?.ToLowerInvariant() switch
        {
            "ar" or "ar-ae" or "ar-sa" => !string.IsNullOrEmpty(Ar) ? Ar : En,
            _ => !string.IsNullOrEmpty(En) ? En : Ar
        };
    }

    /// <summary>
    /// Returns English text as default string representation.
    /// </summary>
    public override string ToString() => En;

    /// <summary>
    /// Implicit conversion from string (creates English-only).
    /// </summary>
    public static implicit operator LocalizedString(string en) => FromEnglish(en);

    /// <summary>
    /// Implicit conversion to string (returns English).
    /// </summary>
    public static implicit operator string(LocalizedString ls) => ls.En;
}
