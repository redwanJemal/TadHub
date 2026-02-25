namespace Candidate.Core.Entities;

/// <summary>
/// How the candidate was sourced.
/// </summary>
public enum CandidateSourceType
{
    /// <summary>
    /// Sourced through a foreign supplier agency. Full procurement pipeline.
    /// </summary>
    Supplier = 0,

    /// <summary>
    /// Walk-in or returning candidate. Short pipeline (skips procurement/travel stages).
    /// </summary>
    Local = 1
}
