namespace Financial.Contracts.DTOs;

public sealed record MarginReportDto
{
    public decimal TotalRevenue { get; init; }
    public decimal TotalCost { get; init; }
    public decimal GrossMargin { get; init; }
    public decimal MarginPercentage { get; init; }
    public List<MarginLineDto> Lines { get; init; } = [];
}

public sealed record MarginLineDto
{
    public Guid? ContractId { get; init; }
    public Guid? WorkerId { get; init; }
    public Guid? ClientId { get; init; }
    public decimal Revenue { get; init; }
    public decimal Cost { get; init; }
    public decimal Margin { get; init; }
    public decimal MarginPercentage { get; init; }
}

public sealed record CashReconciliationDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public DateOnly ReportDate { get; init; }
    public Guid? CashierId { get; init; }
    public string? CashierName { get; init; }
    public decimal CashTotal { get; init; }
    public decimal CardTotal { get; init; }
    public decimal BankTransferTotal { get; init; }
    public decimal ChequeTotal { get; init; }
    public decimal EDirhamTotal { get; init; }
    public decimal OnlineTotal { get; init; }
    public decimal GrandTotal { get; init; }
    public int TransactionCount { get; init; }
    public string? Notes { get; init; }
    public bool IsClosed { get; init; }
    public DateTimeOffset? ClosedAt { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

public sealed record CashReconciliationListDto
{
    public Guid Id { get; init; }
    public DateOnly ReportDate { get; init; }
    public string? CashierName { get; init; }
    public decimal GrandTotal { get; init; }
    public int TransactionCount { get; init; }
    public bool IsClosed { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

public sealed record RevenueBreakdownDto
{
    public decimal TotalRevenue { get; init; }
    public Dictionary<string, decimal> ByPeriod { get; init; } = new();
    public Dictionary<string, decimal> ByPaymentMethod { get; init; } = new();
}
