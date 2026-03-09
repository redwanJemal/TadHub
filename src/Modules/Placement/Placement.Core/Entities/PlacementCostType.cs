namespace Placement.Core.Entities;

public enum PlacementCostType
{
    Procurement = 0,
    Flight = 1,
    Medical = 2,
    Visa = 3,
    EmiratesId = 4,
    Insurance = 5,
    Accommodation = 6,
    Training = 7,
    Other = 8,
}

public enum PlacementCostStatus
{
    Pending = 0,
    Paid = 1,
    Cancelled = 2,
}
