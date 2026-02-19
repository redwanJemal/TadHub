using Microsoft.AspNetCore.Http;
using TadHub.SharedKernel.Localization;

namespace TadHub.Infrastructure.Localization;

/// <summary>
/// Implementation of ILocalizationService that uses HTTP context for locale resolution.
/// </summary>
public class LocalizationService : ILocalizationService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private const string LocaleKey = "X-Locale";
    private const string DefaultLocale = "en";

    public LocalizationService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc />
    public string Resolve(LocalizedString localizedString)
    {
        return localizedString.Resolve(GetCurrentLocale());
    }

    /// <inheritdoc />
    public string Resolve(LocalizedString localizedString, string locale)
    {
        return localizedString.Resolve(locale);
    }

    /// <inheritdoc />
    public string GetCurrentLocale()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
            return DefaultLocale;

        // Check items first (set by SetLocale)
        if (httpContext.Items.TryGetValue(LocaleKey, out var localeObj) && localeObj is string locale)
            return locale;

        // Check Accept-Language header
        var acceptLanguage = httpContext.Request.Headers["Accept-Language"].FirstOrDefault();
        if (!string.IsNullOrEmpty(acceptLanguage))
        {
            // Parse first language preference
            var primaryLanguage = acceptLanguage.Split(',').FirstOrDefault()?.Split(';').FirstOrDefault()?.Trim();
            if (!string.IsNullOrEmpty(primaryLanguage))
                return primaryLanguage.ToLowerInvariant();
        }

        // Check X-Locale header
        var xLocale = httpContext.Request.Headers[LocaleKey].FirstOrDefault();
        if (!string.IsNullOrEmpty(xLocale))
            return xLocale.ToLowerInvariant();

        return DefaultLocale;
    }

    /// <inheritdoc />
    public void SetLocale(string locale)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            httpContext.Items[LocaleKey] = locale.ToLowerInvariant();
        }
    }
}
