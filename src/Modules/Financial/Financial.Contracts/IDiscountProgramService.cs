using Financial.Contracts.DTOs;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Models;

namespace Financial.Contracts;

public interface IDiscountProgramService
{
    Task<PagedList<DiscountProgramListDto>> ListAsync(Guid tenantId, QueryParameters qp, CancellationToken ct = default);
    Task<Result<DiscountProgramDto>> GetByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<Result<DiscountProgramDto>> CreateAsync(Guid tenantId, CreateDiscountProgramRequest request, CancellationToken ct = default);
    Task<Result<DiscountProgramDto>> UpdateAsync(Guid tenantId, Guid id, UpdateDiscountProgramRequest request, CancellationToken ct = default);
    Task<Result> DeleteAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<Result<decimal>> CalculateDiscountAsync(Guid tenantId, Guid programId, decimal baseAmount, CancellationToken ct = default);
}
