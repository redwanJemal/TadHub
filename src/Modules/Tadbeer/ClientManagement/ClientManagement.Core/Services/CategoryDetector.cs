using ClientManagement.Core.Entities;

namespace ClientManagement.Core.Services;

/// <summary>
/// Detects client category from Emirates ID.
/// UAE Emirates ID format: 784-YYYY-NNNNNNN-C
/// First digit after 784 indicates nationality category.
/// </summary>
public static class CategoryDetector
{
    /// <summary>
    /// Detects client category from Emirates ID.
    /// UAE nationals have IDs starting with 784-19XX or specific patterns.
    /// </summary>
    public static ClientCategory DetectFromEmiratesId(string emiratesId)
    {
        if (string.IsNullOrWhiteSpace(emiratesId))
            return ClientCategory.Expat;

        // Normalize: remove dashes and spaces
        var normalized = emiratesId.Replace("-", "").Replace(" ", "");

        // Emirates ID should start with 784 (UAE country code)
        if (!normalized.StartsWith("784"))
            return ClientCategory.Expat;

        // UAE nationals typically have birth year in 1900s-2000s
        // and specific ID patterns
        if (normalized.Length >= 7)
        {
            // Extract year portion (positions 3-6)
            var yearPart = normalized.Substring(3, 4);
            
            if (int.TryParse(yearPart, out var year))
            {
                // UAE nationals born before 1972 (UAE formation)
                // typically have specific patterns
                // This is a simplified heuristic
                
                // Check the 8th digit (nationality indicator in some formats)
                if (normalized.Length >= 15)
                {
                    var nationalityIndicator = normalized[7];
                    
                    // In UAE EID format, certain patterns indicate nationals
                    // 1 = UAE national, other values = residents
                    if (nationalityIndicator == '1')
                        return ClientCategory.Local;
                }
            }
        }

        return ClientCategory.Expat;
    }

    /// <summary>
    /// Validates Emirates ID format.
    /// </summary>
    public static bool IsValidEmiratesId(string emiratesId)
    {
        if (string.IsNullOrWhiteSpace(emiratesId))
            return false;

        var normalized = emiratesId.Replace("-", "").Replace(" ", "");

        // Must be 15 digits
        if (normalized.Length != 15)
            return false;

        // Must be all digits
        if (!normalized.All(char.IsDigit))
            return false;

        // Must start with 784
        if (!normalized.StartsWith("784"))
            return false;

        return true;
    }

    /// <summary>
    /// Formats Emirates ID with dashes: 784-YYYY-NNNNNNN-C
    /// </summary>
    public static string FormatEmiratesId(string emiratesId)
    {
        var normalized = emiratesId.Replace("-", "").Replace(" ", "");
        
        if (normalized.Length != 15)
            return emiratesId;

        return $"{normalized[..3]}-{normalized[3..7]}-{normalized[7..14]}-{normalized[14]}";
    }
}
