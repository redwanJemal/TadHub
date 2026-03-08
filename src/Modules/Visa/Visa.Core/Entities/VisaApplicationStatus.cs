namespace Visa.Core.Entities;

public enum VisaApplicationStatus
{
    NotStarted = 0,
    DocumentsCollecting = 1,
    Applied = 2,
    UnderProcess = 3,
    Approved = 4,
    Rejected = 5,
    Issued = 6,
    Expired = 7,
    Cancelled = 8,
}
