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

public class PaymentService : IPaymentService
{
    private readonly AppDbContext _db;
    private readonly IClock _clock;
    private readonly ICurrentUser _currentUser;
    private readonly IPaymentGateway _gateway;
    private readonly ILogger<PaymentService> _logger;

    private static readonly Dictionary<string, Expression<Func<Payment, object>>> FilterableFields = new()
    {
        ["status"] = x => x.Status,
        ["method"] = x => x.Method,
        ["invoiceId"] = x => x.InvoiceId,
        ["clientId"] = x => x.ClientId,
    };

    private static readonly Dictionary<string, Expression<Func<Payment, object>>> SortableFields = new()
    {
        ["paymentNumber"] = x => x.PaymentNumber,
        ["status"] = x => x.Status,
        ["amount"] = x => x.Amount,
        ["paymentDate"] = x => x.PaymentDate,
        ["createdAt"] = x => x.CreatedAt,
    };

    public PaymentService(
        AppDbContext db,
        IClock clock,
        ICurrentUser currentUser,
        IPaymentGateway gateway,
        ILogger<PaymentService> logger)
    {
        _db = db;
        _clock = clock;
        _currentUser = currentUser;
        _gateway = gateway;
        _logger = logger;
    }

    public async Task<PagedList<PaymentListDto>> ListAsync(Guid tenantId, QueryParameters qp, CancellationToken ct = default)
    {
        var query = _db.Set<Payment>()
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

    public async Task<Result<PaymentDto>> GetByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var payment = await _db.Set<Payment>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct);

        if (payment is null)
            return Result<PaymentDto>.NotFound($"Payment with ID {id} not found");

        return Result<PaymentDto>.Success(MapToDto(payment));
    }

    public async Task<Result<PaymentDto>> RecordPaymentAsync(Guid tenantId, RecordPaymentRequest request, CancellationToken ct = default)
    {
        // Validate payment method
        if (!Enum.TryParse<PaymentMethod>(request.Method, ignoreCase: true, out var method))
            return Result<PaymentDto>.ValidationError($"Invalid payment method '{request.Method}'");

        // Validate invoice exists
        var invoice = await _db.Set<Invoice>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == request.InvoiceId && x.TenantId == tenantId && !x.IsDeleted, ct);

        if (invoice is null)
            return Result<PaymentDto>.NotFound($"Invoice with ID {request.InvoiceId} not found");

        if (invoice.Status == InvoiceStatus.Cancelled || invoice.Status == InvoiceStatus.Refunded)
            return Result<PaymentDto>.ValidationError($"Cannot record payment for a {invoice.Status} invoice");

        if (request.Amount > invoice.BalanceDue)
            return Result<PaymentDto>.ValidationError($"Payment amount ({request.Amount}) exceeds invoice balance due ({invoice.BalanceDue})");

        // Generate payment number
        var paymentNumber = await GeneratePaymentNumberAsync(tenantId, "PAY", ct);

        // Initiate via gateway
        var gatewayProvider = request.GatewayProvider ?? "manual";
        var gatewayResult = await _gateway.InitiatePaymentAsync(new PaymentGatewayRequest
        {
            Amount = request.Amount,
            Currency = request.Currency,
            InvoiceNumber = invoice.InvoiceNumber,
        }, ct);

        var now = _clock.UtcNow;
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PaymentNumber = paymentNumber,
            Status = gatewayResult.Success ? PaymentStatus.Completed : PaymentStatus.Pending,
            InvoiceId = request.InvoiceId,
            ClientId = request.ClientId,
            Amount = request.Amount,
            Currency = request.Currency,
            Method = method,
            ReferenceNumber = request.ReferenceNumber,
            PaymentDate = request.PaymentDate,
            GatewayProvider = gatewayProvider,
            GatewayTransactionId = gatewayResult.TransactionId,
            GatewayStatus = gatewayResult.Status,
            GatewayResponseJson = gatewayResult.RawResponse,
            CashierId = request.CashierId,
            CashierName = request.CashierName,
            Notes = request.Notes,
            CreatedBy = _currentUser.UserId,
        };

        _db.Set<Payment>().Add(payment);

        // Update invoice amounts if payment completed
        if (payment.Status == PaymentStatus.Completed)
        {
            invoice.PaidAmount += request.Amount;
            invoice.BalanceDue = invoice.TotalAmount - invoice.PaidAmount;
            if (invoice.BalanceDue < 0) invoice.BalanceDue = 0;

            // Auto-transition invoice status
            if (invoice.BalanceDue == 0 && invoice.Status != InvoiceStatus.Paid)
            {
                invoice.Status = InvoiceStatus.Paid;
                invoice.StatusChangedAt = now;
            }
            else if (invoice.PaidAmount > 0 && invoice.BalanceDue > 0 &&
                     invoice.Status != InvoiceStatus.PartiallyPaid)
            {
                invoice.Status = InvoiceStatus.PartiallyPaid;
                invoice.StatusChangedAt = now;
            }
        }

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Recorded payment {PaymentNumber} of {Amount} {Currency} for invoice {InvoiceNumber}",
            paymentNumber, request.Amount, request.Currency, invoice.InvoiceNumber);

        return Result<PaymentDto>.Success(MapToDto(payment));
    }

    public async Task<Result<PaymentDto>> TransitionStatusAsync(Guid tenantId, Guid id, TransitionPaymentStatusRequest request, CancellationToken ct = default)
    {
        var payment = await _db.Set<Payment>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct);

        if (payment is null)
            return Result<PaymentDto>.NotFound($"Payment with ID {id} not found");

        if (!Enum.TryParse<PaymentStatus>(request.Status, ignoreCase: true, out var targetStatus))
            return Result<PaymentDto>.ValidationError($"Invalid status '{request.Status}'");

        var error = PaymentStatusMachine.Validate(payment.Status, targetStatus, request.Reason);
        if (error is not null)
            return Result<PaymentDto>.ValidationError(error);

        var fromStatus = payment.Status;
        payment.Status = targetStatus;

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Transitioned payment {PaymentId} from {FromStatus} to {ToStatus}", id, fromStatus, targetStatus);

        return Result<PaymentDto>.Success(MapToDto(payment));
    }

    public async Task<Result<PaymentDto>> RefundPaymentAsync(Guid tenantId, Guid id, RefundPaymentRequest request, CancellationToken ct = default)
    {
        var originalPayment = await _db.Set<Payment>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct);

        if (originalPayment is null)
            return Result<PaymentDto>.NotFound($"Payment with ID {id} not found");

        if (originalPayment.Status != PaymentStatus.Completed)
            return Result<PaymentDto>.ValidationError("Only Completed payments can be refunded");

        if (request.Amount > originalPayment.Amount)
            return Result<PaymentDto>.ValidationError("Refund amount cannot exceed original payment amount");

        // Refund via gateway
        if (originalPayment.GatewayTransactionId is not null)
        {
            await _gateway.RefundAsync(originalPayment.GatewayTransactionId, request.Amount, ct);
        }

        var paymentNumber = await GeneratePaymentNumberAsync(tenantId, "PAY", ct);

        var refundPayment = new Payment
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PaymentNumber = paymentNumber,
            Status = PaymentStatus.Refunded,
            InvoiceId = originalPayment.InvoiceId,
            ClientId = originalPayment.ClientId,
            Amount = request.Amount,
            Currency = originalPayment.Currency,
            Method = originalPayment.Method,
            PaymentDate = DateOnly.FromDateTime(_clock.UtcNow.DateTime),
            RefundedPaymentId = id,
            RefundAmount = request.Amount,
            Notes = $"Refund: {request.Reason}. {request.Notes}".Trim(),
            CreatedBy = _currentUser.UserId,
        };

        _db.Set<Payment>().Add(refundPayment);

        // Update original payment status
        originalPayment.Status = PaymentStatus.Refunded;

        // Update invoice
        var invoice = await _db.Set<Invoice>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == originalPayment.InvoiceId && x.TenantId == tenantId && !x.IsDeleted, ct);

        if (invoice is not null)
        {
            invoice.PaidAmount -= request.Amount;
            if (invoice.PaidAmount < 0) invoice.PaidAmount = 0;
            invoice.BalanceDue = invoice.TotalAmount - invoice.PaidAmount;
            if (invoice.BalanceDue < 0) invoice.BalanceDue = 0;
        }

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Refunded payment {PaymentId}, amount {Amount}", id, request.Amount);

        return Result<PaymentDto>.Success(MapToDto(refundPayment));
    }

    public async Task<Result> DeleteAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var payment = await _db.Set<Payment>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct);

        if (payment is null)
            return Result.NotFound($"Payment with ID {id} not found");

        payment.MarkAsDeleted(_currentUser.UserId);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Soft-deleted payment {PaymentId}", id);

        return Result.Success();
    }

    #region Helpers

    private async Task<string> GeneratePaymentNumberAsync(Guid tenantId, string prefix, CancellationToken ct)
    {
        var lastNumber = await _db.Set<Payment>()
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

        return $"{prefix}-{nextNumber:D6}";
    }

    #endregion

    #region Mapping

    private static PaymentDto MapToDto(Payment p)
    {
        return new PaymentDto
        {
            Id = p.Id,
            TenantId = p.TenantId,
            PaymentNumber = p.PaymentNumber,
            Status = p.Status.ToString(),
            InvoiceId = p.InvoiceId,
            ClientId = p.ClientId,
            Amount = p.Amount,
            Currency = p.Currency,
            Method = p.Method.ToString(),
            ReferenceNumber = p.ReferenceNumber,
            PaymentDate = p.PaymentDate,
            GatewayProvider = p.GatewayProvider,
            GatewayTransactionId = p.GatewayTransactionId,
            GatewayStatus = p.GatewayStatus,
            RefundedPaymentId = p.RefundedPaymentId,
            RefundAmount = p.RefundAmount,
            CashierId = p.CashierId,
            CashierName = p.CashierName,
            Notes = p.Notes,
            CreatedBy = p.CreatedBy,
            UpdatedBy = p.UpdatedBy,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt,
        };
    }

    private static PaymentListDto MapToListDto(Payment p)
    {
        return new PaymentListDto
        {
            Id = p.Id,
            PaymentNumber = p.PaymentNumber,
            Status = p.Status.ToString(),
            InvoiceId = p.InvoiceId,
            ClientId = p.ClientId,
            Amount = p.Amount,
            Currency = p.Currency,
            Method = p.Method.ToString(),
            ReferenceNumber = p.ReferenceNumber,
            PaymentDate = p.PaymentDate,
            CashierName = p.CashierName,
            CreatedAt = p.CreatedAt,
        };
    }

    #endregion
}
