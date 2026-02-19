using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TadHub.SharedKernel.Localization;

namespace TadHub.Infrastructure.Localization;

/// <summary>
/// EF Core value converter for LocalizedString to JSONB.
/// </summary>
public class LocalizedStringConverter : ValueConverter<LocalizedString, string>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public LocalizedStringConverter() : base(
        v => Serialize(v),
        v => Deserialize(v))
    {
    }

    private static string Serialize(LocalizedString value)
    {
        return JsonSerializer.Serialize(value, JsonOptions);
    }

    private static LocalizedString Deserialize(string value)
    {
        if (string.IsNullOrEmpty(value))
            return new LocalizedString();

        try
        {
            return JsonSerializer.Deserialize<LocalizedString>(value, JsonOptions) ?? new LocalizedString();
        }
        catch
        {
            // If JSON is malformed, return empty
            return new LocalizedString();
        }
    }
}

/// <summary>
/// Extension methods for configuring LocalizedString in EF Core.
/// </summary>
public static class LocalizedStringExtensions
{
    /// <summary>
    /// Configures a property to use LocalizedString stored as JSONB.
    /// </summary>
    public static Microsoft.EntityFrameworkCore.Metadata.Builders.PropertyBuilder<LocalizedString> HasLocalizedStringConversion(
        this Microsoft.EntityFrameworkCore.Metadata.Builders.PropertyBuilder<LocalizedString> builder)
    {
        return builder
            .HasConversion(new LocalizedStringConverter())
            .HasColumnType("jsonb");
    }
}
