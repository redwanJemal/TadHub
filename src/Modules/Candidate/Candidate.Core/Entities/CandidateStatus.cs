namespace Candidate.Core.Entities;

/// <summary>
/// Status of a candidate in the recruitment pipeline.
/// </summary>
public enum CandidateStatus
{
    Received = 0,
    UnderReview = 1,
    Approved = 2,
    Rejected = 3,
    Cancelled = 10,
}
