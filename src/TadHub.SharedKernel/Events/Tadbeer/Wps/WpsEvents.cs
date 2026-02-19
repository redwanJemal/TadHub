namespace TadHub.SharedKernel.Events.Tadbeer.Wps;

/// <summary>
/// Published when a SIF (Salary Information File) is submitted to the bank.
/// </summary>
public record SifSubmittedEvent : TadbeerEventBase
{
    public Guid SifFileId { get; init; }
    public string Month { get; init; } = string.Empty; // YYYY-MM
    public int RecordCount { get; init; }
    public decimal TotalAmount { get; init; }
}

/// <summary>
/// Published when there's a WPS compliance alert.
/// E.g., salary not submitted by the 15th of the month.
/// </summary>
public record WpsComplianceAlertEvent : TadbeerEventBase
{
    public string AlertType { get; init; } = string.Empty; // DeadlineApproaching, DeadlineMissed
    public string Month { get; init; } = string.Empty;
    public int DaysUntilDeadline { get; init; } // Negative if past deadline
    public int AffectedWorkerCount { get; init; }
}

/// <summary>
/// Published when a worker's salary is paid via WPS.
/// </summary>
public record SalaryPaidEvent : TadbeerEventBase
{
    public Guid PayrollRecordId { get; init; }
    public Guid WorkerId { get; init; }
    public Guid? ContractId { get; init; }
    public string Month { get; init; } = string.Empty;
    public decimal NetPay { get; init; }
    public string PaymentMethod { get; init; } = string.Empty;
}
