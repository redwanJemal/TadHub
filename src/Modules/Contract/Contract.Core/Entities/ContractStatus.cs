namespace Contract.Core.Entities;

public enum ContractStatus
{
    Draft = 0,
    Confirmed = 1,
    OnProbation = 2,
    Active = 3,
    Completed = 4,
    Terminated = 5,
    Cancelled = 6,
    Closed = 7,
}

public enum RatePeriod
{
    Monthly = 0,
    Daily = 1,
    Hourly = 2,
}

public enum TerminatedByParty
{
    Client = 0,
    Center = 1,
    Worker = 2,
}
