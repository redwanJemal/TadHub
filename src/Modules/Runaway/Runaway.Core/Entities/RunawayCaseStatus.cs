namespace Runaway.Core.Entities;

public enum RunawayCaseStatus
{
    Reported = 0,
    UnderInvestigation = 1,
    Confirmed = 2,
    Settled = 3,
    Closed = 4,
}

public enum GuaranteePeriodType
{
    SixMonths = 0,
    OneYear = 1,
    TwoYears = 2,
}

public enum RunawayExpenseType
{
    CommissionRefund = 0,
    VisaCost = 1,
    MedicalCost = 2,
    TransportationCost = 3,
    Other = 4,
}

public enum PaidByParty
{
    Office = 0,
    Supplier = 1,
    Client = 2,
}
