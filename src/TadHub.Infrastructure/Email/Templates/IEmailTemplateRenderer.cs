namespace TadHub.Infrastructure.Email.Templates;

/// <summary>
/// Renders email templates with data substitution.
/// </summary>
public interface IEmailTemplateRenderer
{
    /// <summary>
    /// Renders a template by name with the given data dictionary.
    /// </summary>
    string Render(string templateName, Dictionary<string, string> data);
}
