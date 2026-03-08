namespace Candidate.Core.Entities;

/// <summary>
/// Classification of candidate location at registration time.
/// </summary>
public enum CandidateLocationType
{
    /// <summary>
    /// Candidate is inside the country (UAE).
    /// </summary>
    InsideCountry = 0,

    /// <summary>
    /// Candidate is outside the country (abroad).
    /// </summary>
    OutsideCountry = 1
}
