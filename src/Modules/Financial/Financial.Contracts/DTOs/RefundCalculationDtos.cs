namespace Financial.Contracts.DTOs;

public sealed record RefundCalculationDto
{
    public Guid ContractId { get; init; }
    public decimal TotalPaid { get; init; }
    public int ContractMonths { get; init; }
    public decimal MonthsWorked { get; init; }
    public decimal ValuePerMonth { get; init; }
    public decimal RefundAmount { get; init; }
    public string PartialMonthMethod { get; init; } = string.Empty;
    public DateOnly ContractStartDate { get; init; }
    public DateOnly ReturnDate { get; init; }
}

public sealed record CommissionCalculationDto
{
    public Guid PlacementId { get; init; }
    public Guid SupplierId { get; init; }
    public string CalculationType { get; init; } = string.Empty;
    public decimal CommissionAmount { get; init; }
    public decimal? ContractValue { get; init; }
    public decimal? Percentage { get; init; }
    public Guid? SupplierPaymentId { get; init; }
}
