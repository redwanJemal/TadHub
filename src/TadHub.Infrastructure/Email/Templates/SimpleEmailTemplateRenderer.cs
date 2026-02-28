using System.Reflection;
using Microsoft.Extensions.Logging;

namespace TadHub.Infrastructure.Email.Templates;

/// <summary>
/// Simple template renderer using string substitution.
/// Loads templates from embedded resources, wraps content in base layout.
/// </summary>
public sealed class SimpleEmailTemplateRenderer : IEmailTemplateRenderer
{
    private readonly ILogger<SimpleEmailTemplateRenderer> _logger;
    private readonly string _layoutTemplate;

    public SimpleEmailTemplateRenderer(ILogger<SimpleEmailTemplateRenderer> logger)
    {
        _logger = logger;
        _layoutTemplate = LoadEmbeddedTemplate("_Layout") ?? GetFallbackLayout();
    }

    public string Render(string templateName, Dictionary<string, string> data)
    {
        var contentTemplate = LoadEmbeddedTemplate(templateName);

        if (contentTemplate == null)
        {
            _logger.LogWarning("Email template '{TemplateName}' not found, using generic", templateName);
            contentTemplate = LoadEmbeddedTemplate("GenericNotification") ?? GetFallbackContent();
        }

        // Substitute variables in content template
        var content = SubstituteVariables(contentTemplate, data);

        // Wrap in layout
        var layoutData = new Dictionary<string, string>(data)
        {
            ["Content"] = content,
            ["Year"] = DateTime.UtcNow.Year.ToString()
        };

        return SubstituteVariables(_layoutTemplate, layoutData);
    }

    private static string SubstituteVariables(string template, Dictionary<string, string> data)
    {
        var result = template;
        foreach (var (key, value) in data)
        {
            result = result.Replace($"{{{{{key}}}}}", value);
        }
        return result;
    }

    private static string? LoadEmbeddedTemplate(string name)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = assembly.GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith($".Templates.{name}.html", StringComparison.OrdinalIgnoreCase));

        if (resourceName == null) return null;

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null) return null;

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private static string GetFallbackLayout() =>
        """
        <!DOCTYPE html>
        <html>
        <head><meta charset="utf-8"/></head>
        <body style="font-family:Arial,sans-serif;margin:0;padding:0;background:#f5f5f5;">
          <div style="max-width:600px;margin:20px auto;background:#fff;border-radius:8px;overflow:hidden;box-shadow:0 2px 4px rgba(0,0,0,0.1);">
            <div style="background:#1a56db;color:#fff;padding:20px 30px;">
              <h1 style="margin:0;font-size:24px;">TadHub</h1>
            </div>
            <div style="padding:30px;">
              {{Content}}
            </div>
            <div style="padding:15px 30px;background:#f9fafb;border-top:1px solid #e5e7eb;font-size:12px;color:#6b7280;text-align:center;">
              &copy; {{Year}} TadHub. All rights reserved.
            </div>
          </div>
        </body>
        </html>
        """;

    private static string GetFallbackContent() =>
        """
        <h2 style="margin:0 0 15px;color:#111827;">{{Title}}</h2>
        <p style="color:#374151;line-height:1.6;">{{Body}}</p>
        """;
}
