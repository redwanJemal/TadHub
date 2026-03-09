using Financial.Contracts.DTOs;
using TadHub.SharedKernel.Models;

namespace Financial.Contracts;

public interface IRefundCalculationService
{
    Task<Result<RefundCalculationDto>> CalculateRefundAsync(
        Guid tenantId,
        Guid contractId,
        DateOnly returnDate,
        int contractMonths,
        string partialMonthMethod,
        CancellationToken ct = default);

    Task<Result<CommissionCalculationDto>> CalculateCommissionAsync(
        Guid tenantId,
        Guid placementId,
        string calculationType,
        decimal fixedAmount,
        decimal percentage,
        CancellationToken ct = default);
}
