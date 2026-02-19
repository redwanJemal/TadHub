namespace TadHub.SharedKernel.Api;

/// <summary>
/// Static helper for parsing and checking include parameters.
/// 
/// Usage in services:
/// var includes = IncludeResolver.Parse(queryParameters.Include);
/// if (includes.Has("client")) 
///     dto.Client = mapper.ToDto(entity.Client);
/// else 
///     dto.Client = mapper.ToRefDto(entity.Client);
/// </summary>
public static class IncludeResolver
{
    /// <summary>
    /// Parses the include query parameter into a set of include names.
    /// </summary>
    /// <param name="include">Comma-separated include string (e.g., "client,worker,skills")</param>
    /// <returns>HashSet of lowercase include names for O(1) lookup</returns>
    public static IncludeSet Parse(string? include)
    {
        if (string.IsNullOrWhiteSpace(include))
            return new IncludeSet(new HashSet<string>());

        var includes = include
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(x => x.ToLowerInvariant())
            .ToHashSet();

        return new IncludeSet(includes);
    }
}

/// <summary>
/// Immutable set of parsed include names with helper methods.
/// </summary>
public readonly struct IncludeSet
{
    private readonly HashSet<string> _includes;

    public IncludeSet(HashSet<string> includes)
    {
        _includes = includes ?? new HashSet<string>();
    }

    /// <summary>
    /// Checks if a specific include is requested (case-insensitive).
    /// </summary>
    public bool Has(string name) => _includes.Contains(name.ToLowerInvariant());

    /// <summary>
    /// Checks if any includes are requested.
    /// </summary>
    public bool Any => _includes.Count > 0;

    /// <summary>
    /// Gets all requested include names.
    /// </summary>
    public IReadOnlySet<string> All => _includes;

    /// <summary>
    /// Returns true if the include set is empty.
    /// </summary>
    public bool IsEmpty => _includes.Count == 0;
}
