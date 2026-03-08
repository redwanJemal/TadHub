namespace Returnee.Core.Entities;

public enum ReturneeCaseStatus
{
    Submitted = 0,
    UnderReview = 1,
    Approved = 2,
    Rejected = 3,
    Settled = 4,
}

public enum ReturnType
{
    ReturnToOffice = 0,
    ReturnToCountry = 1,
}

public enum GuaranteePeriodType
{
    SixMonths = 0,
    OneYear = 1,
    TwoYears = 2,
}

public enum ExpenseType
{
    VisaCost = 0,
    TicketCost = 1,
    MedicalCost = 2,
    TransportationCost = 3,
    AccommodationCost = 4,
    Other = 5,
}

public enum PaidByParty
{
    Office = 0,
    Supplier = 1,
    Client = 2,
}
