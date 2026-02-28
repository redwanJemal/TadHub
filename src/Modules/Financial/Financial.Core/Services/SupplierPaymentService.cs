using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Financial.Contracts;
using Financial.Contracts.DTOs;
using Financial.Core.Entities;
using TadHub.Infrastructure.Api;
using TadHub.Infrastructure.Persistence;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Interfaces;
using TadHub.SharedKernel.Models;

namespace Financial.Core.Services;

public class SupplierPaymentService : ISupplierPaymentService
{
    private readonly AppDbContext _db;
    private readonly IClock _clock;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<SupplierPaymentService> _logger;

    private static readonly Dictionary<string, Expression<Func<SupplierPayment, object>>> FilterableFields = new()
    {
        ["status"] = x => x.Status,
        ["supplierId"] = x => x.SupplierId,
        ["workerId"] = x => x.WorkerId!,
        ["contractId"] = x => x.ContractId!,
    };

    private static readonly Dictionary<string, Expression<Func<SupplierPayment, object>>> SortableFields = new()
    {
        ["paymentNumber"] = x => x.PaymentNumber,
        ["status"] = x => x.Status,
        ["amount"] = x => x.Amount,
        ["paymentDate"] = x => x.PaymentDate,
        ["createdAt"] = x => x.CreatedAt,
    };

    public SupplierPaymentService(
        AppDbContext db,
        IClock clock,
        ICurrentUser currentUser,
        ILogger<SupplierPaymentService> logger)
    {
        _db = db;
        _clock = clock;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<PagedList<SupplierPaymentListDto>> ListAsync(Guid tenantId, QueryParameters qp, CancellationToken ct = default)
    {
        var query = _db.Set<SupplierPayment>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .ApplyFilters(qp.Filters, FilterableFields)
            .ApplySort(qp.GetSortFields(), SortableFields);

        if (!string.IsNullOrWhiteSpace(qp.Search))
        {
            var searchLower = qp.Search.ToLower();
            query = query.Where(x =>
                x.PaymentNumber.ToLower().Contains(searchLower) ||
                (x.ReferenceNumber != null && x.ReferenceNumber.ToLower().Contains(searchLower)));
        }

        return await query
            .Select(x => MapToListDto(x))
            .ToPagedListAsync(qp, ct);
    }

    public async Task<Result<SupplierPaymentDto>> GetByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var payment = await _db.Set<SupplierPayment>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct);

        if (payment is null)
            return Result<SupplierPaymentDto>.NotFound($"Supplier payment with ID {id} not found");

        return Result<SupplierPaymentDto>.Success(MapToDto(payment));
    }

    public async Task<Result<SupplierPaymentDto>> CreateAsync(Guid tenantId, CreateSupplierPaymentRequest request, CancellationToken ct = default)
    {
        if (!Enum.TryParse<PaymentMethod>(request.Method, ignoreCase: true, out var method))
            return Result<SupplierPaymentDto>.ValidationError($"Invalid payment method '{request.Method}'");

        // Generate payment number
        var lastNumber = await _db.Set<SupplierPayment>()
            .IgnoreQueryFilters()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.PaymentNumber)
            .Select(x => x.PaymentNumber)
            .FirstOrDefaultAsync(ct);

        var nextNumber = 1;
        if (lastNumber is not null)
        {
            var dashIndex = lastNumber.LastIndexOf('-');
            if (dashIndex >= 0 && int.TryParse(lastNumber[(dashIndex + 1)..], out var parsed))
                nextNumber = parsed + 1;
        }

        var paymentNumber = $"SPAY-{nextNumber:D6}";

        var payment = new SupplierPayment
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PaymentNumber = paymentNumber,
            Status = SupplierPaymentStatus.Pending,
            SupplierId = request.SupplierId,
            WorkerId = request.WorkerId,
            ContractId = request.ContractId,
            Amount = request.Amount,
            Currency = request.Currency,
            Method = method,
            ReferenceNumber = request.ReferenceNumber,
            PaymentDate = request.PaymentDate,
            Notes = request.Notes,
            CreatedBy = _currentUser.UserId,
        };

        _db.Set<SupplierPayment>().Add(payment);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Created supplier payment {PaymentNumber} for supplier {SupplierId}", paymentNumber, request.SupplierId);

        return Result<SupplierPaymentDto>.Success(MapToDto(payment));
    }

    public async Task<Result<SupplierPaymentDto>> UpdateAsync(Guid tenantId, Guid id, UpdateSupplierPaymentRequest request, CancellationToken ct = default)
    {
        var payment = await _db.Set<SupplierPayment>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct);

        if (payment is null)
            return Result<SupplierPaymentDto>.NotFound($"Supplier payment with ID {id} not found");

        if (request.Amount.HasValue) payment.Amount = request.Amount.Value;
        if (request.Method is not null && Enum.TryParse<PaymentMethod>(request.Method, ignoreCase: true, out var m))
            payment.Method = m;
        if (request.ReferenceNumber is not null) payment.ReferenceNumber = request.ReferenceNumber;
        if (request.PaymentDate.HasValue) payment.PaymentDate = request.PaymentDate.Value;
        if (request.Notes is not null) payment.Notes = request.Notes;

        payment.UpdatedBy = _currentUser.UserId;
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Updated supplier payment {PaymentId}", id);

        return Result<SupplierPaymentDto>.Success(MapToDto(payment));
    }

    public async Task<Result<SupplierPaymentDto>> TransitionStatusAsync(Guid tenantId, Guid id, TransitionSupplierPaymentStatusRequest request, CancellationToken ct = default)
    {
        var payment = await _db.Set<SupplierPayment>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct);

        if (payment is null)
            return Result<SupplierPaymentDto>.NotFound($"Supplier payment with ID {id} not found");

        if (!Enum.TryParse<SupplierPaymentStatus>(request.Status, ignoreCase: true, out var targetStatus))
            return Result<SupplierPaymentDto>.ValidationError($"Invalid status '{request.Status}'");

        var fromStatus = payment.Status;
        payment.Status = targetStatus;

        if (targetStatus == SupplierPaymentStatus.Paid)
            payment.PaidAt = _clock.UtcNow;

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Transitioned supplier payment {PaymentId} from {FromStatus} to {ToStatus}", id, fromStatus, targetStatus);

        return Result<SupplierPaymentDto>.Success(MapToDto(payment));
    }

    public async Task<Result> DeleteAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var payment = await _db.Set<SupplierPayment>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct);

        if (payment is null)
            return Result.NotFound($"Supplier payment with ID {id} not found");

        payment.MarkAsDeleted(_currentUser.UserId);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Soft-deleted supplier payment {PaymentId}", id);

        return Result.Success();
    }

    #region Mapping

    private static SupplierPaymentDto MapToDto(SupplierPayment p)
    {
        return new SupplierPaymentDto
        {
            Id = p.Id,
            TenantId = p.TenantId,
            PaymentNumber = p.PaymentNumber,
            Status = p.Status.ToString(),
            SupplierId = p.SupplierId,
            WorkerId = p.WorkerId,
            ContractId = p.ContractId,
            Amount = p.Amount,
            Currency = p.Currency,
            Method = p.Method.ToString(),
            ReferenceNumber = p.ReferenceNumber,
            PaymentDate = p.PaymentDate,
            PaidAt = p.PaidAt,
            Notes = p.Notes,
            CreatedBy = p.CreatedBy,
            UpdatedBy = p.UpdatedBy,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt,
        };
    }

    private static SupplierPaymentListDto MapToListDto(SupplierPayment p)
    {
        return new SupplierPaymentListDto
        {
            Id = p.Id,
            PaymentNumber = p.PaymentNumber,
            Status = p.Status.ToString(),
            SupplierId = p.SupplierId,
            WorkerId = p.WorkerId,
            ContractId = p.ContractId,
            Amount = p.Amount,
            Currency = p.Currency,
            Method = p.Method.ToString(),
            PaymentDate = p.PaymentDate,
            CreatedAt = p.CreatedAt,
        };
    }

    #endregion
}
