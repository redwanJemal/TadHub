namespace Reporting.Contracts.DTOs;

// ── Workforce Reports ──

public sealed record InventoryReportItemDto
{
    public Guid Id { get; init; }
    public string WorkerCode { get; init; } = "";
    public string FullNameEn { get; init; } = "";
    public string FullNameAr { get; init; } = "";
    public string? Nationality { get; init; }
    public string Location { get; init; } = "";
    public string Status { get; init; } = "";
    public string? Gender { get; init; }
    public DateOnly? DateOfBirth { get; init; }
    public int? ExperienceYears { get; init; }
    public decimal? MonthlySalary { get; init; }
    public Guid? TenantSupplierId { get; init; }
    public string? SupplierNameEn { get; init; }
    public string? SupplierNameAr { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

public sealed record DeployedReportItemDto
{
    public Guid WorkerId { get; init; }
    public string WorkerCode { get; init; } = "";
    public string FullNameEn { get; init; } = "";
    public string FullNameAr { get; init; } = "";
    public string? Nationality { get; init; }
    public Guid? ContractId { get; init; }
    public string? ContractCode { get; init; }
    public Guid? ClientId { get; init; }
    public string? ClientNameEn { get; init; }
    public string? ClientNameAr { get; init; }
    public DateOnly? StartDate { get; init; }
    public DateOnly? EndDate { get; init; }
    public string? ContractType { get; init; }
    public decimal? Rate { get; init; }
    public string? RatePeriod { get; init; }
}

public sealed record ReturneeReportItemDto
{
    public Guid Id { get; init; }
    public string CaseCode { get; init; } = "";
    public string Status { get; init; } = "";
    public string? ReturnType { get; init; }
    public DateOnly? ReturnDate { get; init; }
    public string? ReturnReason { get; init; }
    public Guid? WorkerId { get; init; }
    public string? WorkerNameEn { get; init; }
    public string? WorkerNameAr { get; init; }
    public Guid? ClientId { get; init; }
    public string? ClientNameEn { get; init; }
    public string? ClientNameAr { get; init; }
    public decimal? TotalAmountPaid { get; init; }
    public decimal? RefundAmount { get; init; }
    public bool IsWithinGuarantee { get; init; }
    public string? GuaranteePeriodType { get; init; }
    public DateTimeOffset? SettledAt { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

public sealed record RunawayReportItemDto
{
    public Guid Id { get; init; }
    public string CaseCode { get; init; } = "";
    public string Status { get; init; } = "";
    public DateTimeOffset? ReportedDate { get; init; }
    public Guid? WorkerId { get; init; }
    public string? WorkerNameEn { get; init; }
    public string? WorkerNameAr { get; init; }
    public Guid? ClientId { get; init; }
    public string? ClientNameEn { get; init; }
    public string? ClientNameAr { get; init; }
    public bool IsWithinGuarantee { get; init; }
    public string? GuaranteePeriodType { get; init; }
    public string? PoliceReportNumber { get; init; }
    public decimal TotalExpenses { get; init; }
    public DateTimeOffset? SettledAt { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

// ── Operational Reports ──

public sealed record ArrivalReportItemDto
{
    public Guid Id { get; init; }
    public string ArrivalCode { get; init; } = "";
    public string Status { get; init; } = "";
    public Guid? WorkerId { get; init; }
    public string? WorkerNameEn { get; init; }
    public string? WorkerNameAr { get; init; }
    public string? FlightNumber { get; init; }
    public string? AirportName { get; init; }
    public DateOnly? ScheduledArrivalDate { get; init; }
    public TimeOnly? ScheduledArrivalTime { get; init; }
    public TimeOnly? ActualArrivalTime { get; init; }
    public string? DriverName { get; init; }
    public DateTimeOffset? DriverConfirmedPickupAt { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

public sealed record AccommodationDailyItemDto
{
    public Guid Id { get; init; }
    public string StayCode { get; init; } = "";
    public string Status { get; init; } = "";
    public Guid? WorkerId { get; init; }
    public string? WorkerNameEn { get; init; }
    public string? WorkerNameAr { get; init; }
    public string? Room { get; init; }
    public string? LocationName { get; init; }
    public DateOnly CheckInDate { get; init; }
    public DateOnly? CheckOutDate { get; init; }
    public string? DepartureReason { get; init; }
}

public sealed record DeploymentPipelineItemDto
{
    public string Stage { get; init; } = "";
    public int Count { get; init; }
}

// ── Finance Reports (Extensions) ──

public sealed record SupplierCommissionItemDto
{
    public Guid SupplierId { get; init; }
    public string? SupplierNameEn { get; init; }
    public string? SupplierNameAr { get; init; }
    public int PaymentCount { get; init; }
    public decimal TotalPaid { get; init; }
    public decimal TotalPending { get; init; }
}

public sealed record RefundReportItemDto
{
    public Guid PaymentId { get; init; }
    public string PaymentNumber { get; init; } = "";
    public string Status { get; init; } = "";
    public decimal Amount { get; init; }
    public decimal? RefundAmount { get; init; }
    public string? Method { get; init; }
    public DateOnly PaymentDate { get; init; }
    public Guid? ClientId { get; init; }
    public string? ClientNameEn { get; init; }
    public string? ClientNameAr { get; init; }
    public Guid? InvoiceId { get; init; }
    public string? InvoiceNumber { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

public sealed record CostPerMaidItemDto
{
    public Guid WorkerId { get; init; }
    public string? WorkerCode { get; init; }
    public string? WorkerNameEn { get; init; }
    public string? WorkerNameAr { get; init; }
    public decimal ProcurementCost { get; init; }
    public decimal FlightCost { get; init; }
    public decimal MedicalCost { get; init; }
    public decimal VisaCost { get; init; }
    public decimal InsuranceCost { get; init; }
    public decimal AccommodationCost { get; init; }
    public decimal TrainingCost { get; init; }
    public decimal OtherCost { get; init; }
    public decimal TotalCost { get; init; }
}

// ── Shared ──

public sealed record ReportSummaryDto
{
    public int TotalCount { get; init; }
    public Dictionary<string, int> CountByStatus { get; init; } = new();
    public decimal? TotalAmount { get; init; }
}
