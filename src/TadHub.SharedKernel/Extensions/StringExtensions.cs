using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace TadHub.SharedKernel.Extensions;

/// <summary>
/// Extension methods for string manipulation.
/// </summary>
public static partial class StringExtensions
{
    /// <summary>
    /// Converts a string to a URL-friendly slug.
    /// Example: "Hello World!" → "hello-world"
    /// </summary>
    public static string ToSlug(this string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        // Normalize and remove diacritics
        var normalized = value.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();

        foreach (var c in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(c);
            if (category != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(c);
            }
        }

        var result = sb.ToString().Normalize(NormalizationForm.FormC);

        // Convert to lowercase
        result = result.ToLowerInvariant();

        // Replace spaces with hyphens
        result = result.Replace(' ', '-');

        // Remove invalid characters (keep only letters, numbers, hyphens)
        result = SlugRegex().Replace(result, "");

        // Replace multiple hyphens with single hyphen
        result = MultipleHyphensRegex().Replace(result, "-");

        // Trim hyphens from start and end
        result = result.Trim('-');

        return result;
    }

    /// <summary>
    /// Truncates a string to the specified maximum length.
    /// Adds "..." if truncated.
    /// </summary>
    public static string Truncate(this string value, int maxLength, string suffix = "...")
    {
        if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
            return value;

        return value[..(maxLength - suffix.Length)] + suffix;
    }

    /// <summary>
    /// Checks if the string is null, empty, or whitespace.
    /// </summary>
    public static bool IsNullOrWhiteSpace(this string? value) =>
        string.IsNullOrWhiteSpace(value);

    /// <summary>
    /// Returns null if the string is empty or whitespace.
    /// </summary>
    public static string? NullIfEmpty(this string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;

    /// <summary>
    /// Converts a string to title case.
    /// </summary>
    public static string ToTitleCase(this string value) =>
        CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value.ToLower());

    [GeneratedRegex("[^a-z0-9-]")]
    private static partial Regex SlugRegex();

    [GeneratedRegex("-{2,}")]
    private static partial Regex MultipleHyphensRegex();
}
