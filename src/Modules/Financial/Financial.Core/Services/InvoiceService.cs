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

public class InvoiceService : IInvoiceService
{
    private readonly AppDbContext _db;
    private readonly IClock _clock;
    private readonly ICurrentUser _currentUser;
    private readonly IDiscountProgramService _discountService;
    private readonly ILogger<InvoiceService> _logger;

    private static readonly Dictionary<string, Expression<Func<Invoice, object>>> FilterableFields = new()
    {
        ["status"] = x => x.Status,
        ["type"] = x => x.Type,
        ["clientId"] = x => x.ClientId,
        ["contractId"] = x => x.ContractId,
    };

    private static readonly Dictionary<string, Expression<Func<Invoice, object>>> SortableFields = new()
    {
        ["invoiceNumber"] = x => x.InvoiceNumber,
        ["status"] = x => x.Status,
        ["issueDate"] = x => x.IssueDate,
        ["dueDate"] = x => x.DueDate,
        ["totalAmount"] = x => x.TotalAmount,
        ["createdAt"] = x => x.CreatedAt,
        ["updatedAt"] = x => x.UpdatedAt,
    };

    public InvoiceService(
        AppDbContext db,
        IClock clock,
        ICurrentUser currentUser,
        IDiscountProgramService discountService,
        ILogger<InvoiceService> logger)
    {
        _db = db;
        _clock = clock;
        _currentUser = currentUser;
        _discountService = discountService;
        _logger = logger;
    }

    public async Task<PagedList<InvoiceListDto>> ListAsync(Guid tenantId, QueryParameters qp, CancellationToken ct = default)
    {
        var query = _db.Set<Invoice>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .ApplyFilters(qp.Filters, FilterableFields)
            .ApplySort(qp.GetSortFields(), SortableFields);

        if (!string.IsNullOrWhiteSpace(qp.Search))
        {
            var searchLower = qp.Search.ToLower();
            query = query.Where(x =>
                x.InvoiceNumber.ToLower().Contains(searchLower));
        }

        return await query
            .Select(x => MapToListDto(x))
            .ToPagedListAsync(qp, ct);
    }

    public async Task<Result<InvoiceDto>> GetByIdAsync(Guid tenantId, Guid id, QueryParameters? qp = null, CancellationToken ct = default)
    {
        var includes = qp?.GetIncludeList() ?? [];
        var includeLineItems = includes.Contains("lineItems", StringComparer.OrdinalIgnoreCase);
        var includePayments = includes.Contains("payments", StringComparer.OrdinalIgnoreCase);

        var query = _db.Set<Invoice>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted);

        if (includeLineItems)
            query = query.Include(x => x.LineItems.OrderBy(li => li.LineNumber));

        if (includePayments)
            query = query.Include(x => x.Payments.OrderByDescending(p => p.CreatedAt));

        var invoice = await query.FirstOrDefaultAsync(x => x.Id == id, ct);

        if (invoice is null)
            return Result<InvoiceDto>.NotFound($"Invoice with ID {id} not found");

        return Result<InvoiceDto>.Success(MapToDto(invoice, includeLineItems, includePayments));
    }

    public async Task<Result<InvoiceDto>> CreateAsync(Guid tenantId, CreateInvoiceRequest request, CancellationToken ct = default)
    {
        // Validate invoice type
        var invoiceType = InvoiceType.Standard;
        if (request.Type is not null && !Enum.TryParse<InvoiceType>(request.Type, ignoreCase: true, out invoiceType))
            return Result<InvoiceDto>.ValidationError($"Invalid invoice type '{request.Type}'");

        // Validate milestone type
        MilestoneType? milestoneType = null;
        if (request.MilestoneType is not null)
        {
            if (!Enum.TryParse<MilestoneType>(request.MilestoneType, ignoreCase: true, out var mt))
                return Result<InvoiceDto>.ValidationError($"Invalid milestone type '{request.MilestoneType}'");
            milestoneType = mt;
        }

        // Generate invoice number
        var invoiceNumber = await GenerateInvoiceNumberAsync(tenantId, "INV", ct);

        var now = _clock.UtcNow;
        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            InvoiceNumber = invoiceNumber,
            Type = invoiceType,
            Status = InvoiceStatus.Draft,
            StatusChangedAt = now,
            ContractId = request.ContractId,
            ClientId = request.ClientId,
            WorkerId = request.WorkerId,
            IssueDate = request.IssueDate,
            DueDate = request.DueDate,
            Currency = request.Currency,
            TenantTrn = request.TenantTrn,
            ClientTrn = request.ClientTrn,
            MilestoneType = milestoneType,
            Notes = request.Notes,
            CreatedBy = _currentUser.UserId,
        };

        // Add line items
        var lineNumber = 1;
        foreach (var li in request.LineItems)
        {
            var lineTotal = (li.Quantity * li.UnitPrice) - li.DiscountAmount;
            invoice.LineItems.Add(new InvoiceLineItem
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                InvoiceId = invoice.Id,
                LineNumber = lineNumber++,
                Description = li.Description,
                DescriptionAr = li.DescriptionAr,
                Quantity = li.Quantity,
                UnitPrice = li.UnitPrice,
                DiscountAmount = li.DiscountAmount,
                LineTotal = lineTotal,
                ItemCode = li.ItemCode,
            });
        }

        RecalculateAmounts(invoice);

        _db.Set<Invoice>().Add(invoice);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Created invoice {InvoiceNumber} for contract {ContractId}", invoiceNumber, request.ContractId);

        return Result<InvoiceDto>.Success(MapToDto(invoice, includeLineItems: true, includePayments: false));
    }

    public async Task<Result<InvoiceDto>> GenerateForContractAsync(Guid tenantId, GenerateInvoiceRequest request, CancellationToken ct = default)
    {
        // Validate milestone type
        MilestoneType? milestoneType = null;
        if (request.MilestoneType is not null)
        {
            if (!Enum.TryParse<MilestoneType>(request.MilestoneType, ignoreCase: true, out var mt))
                return Result<InvoiceDto>.ValidationError($"Invalid milestone type '{request.MilestoneType}'");
            milestoneType = mt;
        }

        // Generate invoice via CreateAsync with a single line item
        var amount = request.OverrideAmount ?? 0;
        var createRequest = new CreateInvoiceRequest
        {
            ContractId = request.ContractId,
            ClientId = Guid.Empty, // Will be set from contract if needed
            IssueDate = DateOnly.FromDateTime(_clock.UtcNow.DateTime),
            DueDate = DateOnly.FromDateTime(_clock.UtcNow.DateTime.AddDays(30)),
            MilestoneType = request.MilestoneType,
            Notes = request.Notes,
            LineItems = amount > 0
                ? [new CreateInvoiceLineItemRequest
                {
                    Description = milestoneType?.ToString() ?? "Contract Service",
                    Quantity = 1,
                    UnitPrice = amount,
                }]
                : [],
        };

        return await CreateAsync(tenantId, createRequest, ct);
    }

    public async Task<Result<InvoiceDto>> UpdateAsync(Guid tenantId, Guid id, UpdateInvoiceRequest request, CancellationToken ct = default)
    {
        var invoice = await _db.Set<Invoice>()
            .IgnoreQueryFilters()
            .Include(x => x.LineItems)
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct);

        if (invoice is null)
            return Result<InvoiceDto>.NotFound($"Invoice with ID {id} not found");

        if (invoice.Status != InvoiceStatus.Draft)
            return Result<InvoiceDto>.ValidationError("Only Draft invoices can be updated");

        if (request.IssueDate.HasValue) invoice.IssueDate = request.IssueDate.Value;
        if (request.DueDate.HasValue) invoice.DueDate = request.DueDate.Value;
        if (request.TenantTrn is not null) invoice.TenantTrn = request.TenantTrn;
        if (request.ClientTrn is not null) invoice.ClientTrn = request.ClientTrn;
        if (request.Notes is not null) invoice.Notes = request.Notes;

        if (request.LineItems is not null)
        {
            // Replace all line items
            _db.Set<InvoiceLineItem>().RemoveRange(invoice.LineItems);
            invoice.LineItems.Clear();

            var lineNumber = 1;
            foreach (var li in request.LineItems)
            {
                var lineTotal = (li.Quantity * li.UnitPrice) - li.DiscountAmount;
                invoice.LineItems.Add(new InvoiceLineItem
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    InvoiceId = invoice.Id,
                    LineNumber = lineNumber++,
                    Description = li.Description,
                    DescriptionAr = li.DescriptionAr,
                    Quantity = li.Quantity,
                    UnitPrice = li.UnitPrice,
                    DiscountAmount = li.DiscountAmount,
                    LineTotal = lineTotal,
                    ItemCode = li.ItemCode,
                });
            }

            RecalculateAmounts(invoice);
        }

        invoice.UpdatedBy = _currentUser.UserId;
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Updated invoice {InvoiceId}", id);

        return Result<InvoiceDto>.Success(MapToDto(invoice, includeLineItems: true, includePayments: false));
    }

    public async Task<Result<InvoiceDto>> TransitionStatusAsync(Guid tenantId, Guid id, TransitionInvoiceStatusRequest request, CancellationToken ct = default)
    {
        var invoice = await _db.Set<Invoice>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct);

        if (invoice is null)
            return Result<InvoiceDto>.NotFound($"Invoice with ID {id} not found");

        if (!Enum.TryParse<InvoiceStatus>(request.Status, ignoreCase: true, out var targetStatus))
            return Result<InvoiceDto>.ValidationError($"Invalid status '{request.Status}'");

        var error = InvoiceStatusMachine.Validate(invoice.Status, targetStatus, request.Reason);
        if (error is not null)
            return Result<InvoiceDto>.ValidationError(error);

        var fromStatus = invoice.Status;
        invoice.Status = targetStatus;
        invoice.StatusChangedAt = _clock.UtcNow;

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Transitioned invoice {InvoiceId} from {FromStatus} to {ToStatus}", id, fromStatus, targetStatus);

        return Result<InvoiceDto>.Success(MapToDto(invoice, includeLineItems: false, includePayments: false));
    }

    public async Task<Result<InvoiceDto>> CreateCreditNoteAsync(Guid tenantId, Guid invoiceId, CreateCreditNoteRequest request, CancellationToken ct = default)
    {
        var originalInvoice = await _db.Set<Invoice>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Include(x => x.LineItems)
            .FirstOrDefaultAsync(x => x.Id == invoiceId && x.TenantId == tenantId && !x.IsDeleted, ct);

        if (originalInvoice is null)
            return Result<InvoiceDto>.NotFound($"Invoice with ID {invoiceId} not found");

        if (originalInvoice.Status != InvoiceStatus.Paid && originalInvoice.Status != InvoiceStatus.PartiallyPaid)
            return Result<InvoiceDto>.ValidationError("Credit notes can only be created for Paid or PartiallyPaid invoices");

        var creditAmount = request.Amount ?? originalInvoice.TotalAmount;
        var invoiceNumber = await GenerateInvoiceNumberAsync(tenantId, "INV", ct);

        var now = _clock.UtcNow;
        var creditNote = new Invoice
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            InvoiceNumber = invoiceNumber,
            Type = InvoiceType.CreditNote,
            Status = InvoiceStatus.Draft,
            StatusChangedAt = now,
            ContractId = originalInvoice.ContractId,
            ClientId = originalInvoice.ClientId,
            WorkerId = originalInvoice.WorkerId,
            IssueDate = DateOnly.FromDateTime(now.DateTime),
            DueDate = DateOnly.FromDateTime(now.DateTime),
            OriginalInvoiceId = invoiceId,
            CreditNoteReason = request.Reason,
            Currency = originalInvoice.Currency,
            Notes = request.Notes,
            CreatedBy = _currentUser.UserId,
        };

        creditNote.LineItems.Add(new InvoiceLineItem
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            InvoiceId = creditNote.Id,
            LineNumber = 1,
            Description = $"Credit Note - {request.Reason}",
            Quantity = 1,
            UnitPrice = creditAmount,
            LineTotal = creditAmount,
        });

        RecalculateAmounts(creditNote);

        _db.Set<Invoice>().Add(creditNote);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Created credit note {InvoiceNumber} for original invoice {OriginalInvoiceId}", invoiceNumber, invoiceId);

        return Result<InvoiceDto>.Success(MapToDto(creditNote, includeLineItems: true, includePayments: false));
    }

    public async Task<Result<InvoiceDto>> ApplyDiscountAsync(Guid tenantId, Guid invoiceId, ApplyDiscountRequest request, CancellationToken ct = default)
    {
        var invoice = await _db.Set<Invoice>()
            .IgnoreQueryFilters()
            .Include(x => x.LineItems)
            .FirstOrDefaultAsync(x => x.Id == invoiceId && x.TenantId == tenantId && !x.IsDeleted, ct);

        if (invoice is null)
            return Result<InvoiceDto>.NotFound($"Invoice with ID {invoiceId} not found");

        if (invoice.Status != InvoiceStatus.Draft)
            return Result<InvoiceDto>.ValidationError("Discounts can only be applied to Draft invoices");

        var discountResult = await _discountService.GetByIdAsync(tenantId, request.DiscountProgramId, ct);
        if (!discountResult.IsSuccess)
            return Result<InvoiceDto>.NotFound("Discount program not found");

        var program = discountResult.Value!;
        var discountAmountResult = await _discountService.CalculateDiscountAsync(tenantId, request.DiscountProgramId, invoice.Subtotal, ct);
        if (!discountAmountResult.IsSuccess)
            return Result<InvoiceDto>.ValidationError(discountAmountResult.Error!);

        invoice.DiscountProgramId = request.DiscountProgramId;
        invoice.DiscountProgramName = program.Name;
        invoice.DiscountCardNumber = request.CardNumber;
        invoice.DiscountPercentage = program.DiscountPercentage;
        invoice.DiscountAmount = discountAmountResult.Value;

        RecalculateAmounts(invoice);

        invoice.UpdatedBy = _currentUser.UserId;
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Applied discount program {ProgramName} to invoice {InvoiceId}", program.Name, invoiceId);

        return Result<InvoiceDto>.Success(MapToDto(invoice, includeLineItems: true, includePayments: false));
    }

    public async Task<Result> DeleteAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var invoice = await _db.Set<Invoice>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct);

        if (invoice is null)
            return Result.NotFound($"Invoice with ID {id} not found");

        invoice.MarkAsDeleted(_currentUser.UserId);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Soft-deleted invoice {InvoiceId}", id);

        return Result.Success();
    }

    public async Task<Result<InvoiceSummaryDto>> GetSummaryAsync(Guid tenantId, CancellationToken ct = default)
    {
        var invoices = await _db.Set<Invoice>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .ToListAsync(ct);

        var summary = new InvoiceSummaryDto
        {
            TotalInvoices = invoices.Count,
            TotalRevenue = invoices.Sum(x => x.TotalAmount),
            TotalPaid = invoices.Sum(x => x.PaidAmount),
            TotalOutstanding = invoices.Sum(x => x.BalanceDue),
            OverdueCount = invoices.Count(x => x.Status == InvoiceStatus.Overdue),
            OverdueAmount = invoices.Where(x => x.Status == InvoiceStatus.Overdue).Sum(x => x.BalanceDue),
            CountsByStatus = invoices.GroupBy(x => x.Status.ToString()).ToDictionary(g => g.Key, g => g.Count()),
        };

        return Result<InvoiceSummaryDto>.Success(summary);
    }

    #region Helpers

    private async Task<string> GenerateInvoiceNumberAsync(Guid tenantId, string prefix, CancellationToken ct)
    {
        var lastNumber = await _db.Set<Invoice>()
            .IgnoreQueryFilters()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.InvoiceNumber)
            .Select(x => x.InvoiceNumber)
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

    private static void RecalculateAmounts(Invoice invoice)
    {
        invoice.Subtotal = invoice.LineItems.Sum(x => x.LineTotal);
        invoice.TaxableAmount = invoice.Subtotal - invoice.DiscountAmount;
        if (invoice.TaxableAmount < 0) invoice.TaxableAmount = 0;
        invoice.VatAmount = Math.Round(invoice.TaxableAmount * invoice.VatRate / 100m, 2);
        invoice.TotalAmount = invoice.TaxableAmount + invoice.VatAmount;
        invoice.BalanceDue = invoice.TotalAmount - invoice.PaidAmount;
        if (invoice.BalanceDue < 0) invoice.BalanceDue = 0;
    }

    #endregion

    #region Mapping

    private static InvoiceDto MapToDto(Invoice inv, bool includeLineItems, bool includePayments)
    {
        return new InvoiceDto
        {
            Id = inv.Id,
            TenantId = inv.TenantId,
            InvoiceNumber = inv.InvoiceNumber,
            Type = inv.Type.ToString(),
            Status = inv.Status.ToString(),
            StatusChangedAt = inv.StatusChangedAt,
            ContractId = inv.ContractId,
            ClientId = inv.ClientId,
            WorkerId = inv.WorkerId,
            IssueDate = inv.IssueDate,
            DueDate = inv.DueDate,
            Subtotal = inv.Subtotal,
            DiscountAmount = inv.DiscountAmount,
            TaxableAmount = inv.TaxableAmount,
            VatRate = inv.VatRate,
            VatAmount = inv.VatAmount,
            TotalAmount = inv.TotalAmount,
            PaidAmount = inv.PaidAmount,
            BalanceDue = inv.BalanceDue,
            Currency = inv.Currency,
            TenantTrn = inv.TenantTrn,
            ClientTrn = inv.ClientTrn,
            DiscountProgramId = inv.DiscountProgramId,
            DiscountProgramName = inv.DiscountProgramName,
            DiscountCardNumber = inv.DiscountCardNumber,
            DiscountPercentage = inv.DiscountPercentage,
            MilestoneType = inv.MilestoneType?.ToString(),
            OriginalInvoiceId = inv.OriginalInvoiceId,
            CreditNoteReason = inv.CreditNoteReason,
            Notes = inv.Notes,
            CreatedBy = inv.CreatedBy,
            UpdatedBy = inv.UpdatedBy,
            CreatedAt = inv.CreatedAt,
            UpdatedAt = inv.UpdatedAt,
            LineItems = includeLineItems
                ? inv.LineItems.Select(MapToLineItemDto).ToList()
                : null,
            Payments = includePayments
                ? inv.Payments.Select(MapToPaymentListDto).ToList()
                : null,
        };
    }

    private static InvoiceListDto MapToListDto(Invoice inv)
    {
        return new InvoiceListDto
        {
            Id = inv.Id,
            InvoiceNumber = inv.InvoiceNumber,
            Type = inv.Type.ToString(),
            Status = inv.Status.ToString(),
            ContractId = inv.ContractId,
            ClientId = inv.ClientId,
            WorkerId = inv.WorkerId,
            IssueDate = inv.IssueDate,
            DueDate = inv.DueDate,
            TotalAmount = inv.TotalAmount,
            PaidAmount = inv.PaidAmount,
            BalanceDue = inv.BalanceDue,
            Currency = inv.Currency,
            MilestoneType = inv.MilestoneType?.ToString(),
            CreatedAt = inv.CreatedAt,
        };
    }

    private static InvoiceLineItemDto MapToLineItemDto(InvoiceLineItem li)
    {
        return new InvoiceLineItemDto
        {
            Id = li.Id,
            LineNumber = li.LineNumber,
            Description = li.Description,
            DescriptionAr = li.DescriptionAr,
            Quantity = li.Quantity,
            UnitPrice = li.UnitPrice,
            DiscountAmount = li.DiscountAmount,
            LineTotal = li.LineTotal,
            ItemCode = li.ItemCode,
        };
    }

    private static PaymentListDto MapToPaymentListDto(Payment p)
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
