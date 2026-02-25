namespace Candidate.Core.Entities;

/// <summary>
/// Status of a candidate in the recruitment pipeline.
/// </summary>
public enum CandidateStatus
{
    // Pipeline stages
    Received = 0,
    UnderReview = 1,
    Approved = 2,
    Rejected = 3,
    ProcurementPaid = 4,
    InTransit = 5,
    Arrived = 6,
    Converted = 7,

    // Terminal/failure statuses
    Cancelled = 10,
    FailedMedicalAbroad = 11,
    VisaDenied = 12,
    ReturnedAfterArrival = 13
}
