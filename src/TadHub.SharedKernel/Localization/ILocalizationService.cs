namespace TadHub.SharedKernel.Localization;

/// <summary>
/// Service for resolving localized strings to the current locale.
/// </summary>
public interface ILocalizationService
{
    /// <summary>
    /// Resolves a localized string to the appropriate language.
    /// Uses tenant configuration or user preference for locale.
    /// </summary>
    string Resolve(LocalizedString localizedString);

    /// <summary>
    /// Resolves a localized string with explicit locale override.
    /// </summary>
    string Resolve(LocalizedString localizedString, string locale);

    /// <summary>
    /// Gets the current locale.
    /// </summary>
    string GetCurrentLocale();

    /// <summary>
    /// Sets the locale for the current request/scope.
    /// </summary>
    void SetLocale(string locale);
}
