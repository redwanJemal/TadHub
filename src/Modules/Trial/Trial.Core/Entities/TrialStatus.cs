namespace Trial.Core.Entities;

public enum TrialStatus
{
    Active = 0,
    Successful = 1,
    Failed = 2,
    Cancelled = 3,
}

public enum TrialOutcome
{
    ProceedToContract = 0,
    ReturnToInventory = 1,
}
