using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Financial.Contracts;
using Financial.Contracts.DTOs;
using Client.Contracts;
using TadHub.Api.Filters;
using TadHub.Infrastructure.Auth;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Models;

namespace TadHub.Api.Controllers;

[ApiController]
[Route("api/v1/tenants/{tenantId:guid}/payments")]
[Authorize]
[TenantMemberRequired(TenantIdParameter = "tenantId")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly IInvoiceService _invoiceService;
    private readonly IClientService _clientService;

    public PaymentsController(
        IPaymentService paymentService,
        IInvoiceService invoiceService,
        IClientService clientService)
    {
        _paymentService = paymentService;
        _invoiceService = invoiceService;
        _clientService = clientService;
    }

    [HttpGet]
    [HasPermission("payments.view")]
    [ProducesResponseType(typeof(PagedList<PaymentListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        Guid tenantId,
        [FromQuery] QueryParameters qp,
        CancellationToken ct)
    {
        var result = await _paymentService.ListAsync(tenantId, qp, ct);
        result = await EnrichList(tenantId, result, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [HasPermission("payments.view")]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid tenantId,
        Guid id,
        CancellationToken ct)
    {
        var result = await _paymentService.GetByIdAsync(tenantId, id, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        var dto = await EnrichSingle(tenantId, result.Value!, ct);
        return Ok(dto);
    }

    [HttpPost]
    [HasPermission("payments.create")]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RecordPayment(
        Guid tenantId,
        [FromBody] RecordPaymentRequest request,
        CancellationToken ct)
    {
        var result = await _paymentService.RecordPaymentAsync(tenantId, request, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        var dto = result.Value!;
        return CreatedAtAction(nameof(GetById), new { tenantId, id = dto.Id }, dto);
    }

    [HttpPost("{id:guid}/status")]
    [HasPermission("payments.manage_status")]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TransitionStatus(
        Guid tenantId,
        Guid id,
        [FromBody] TransitionPaymentStatusRequest request,
        CancellationToken ct)
    {
        var result = await _paymentService.TransitionStatusAsync(tenantId, id, request, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        return Ok(result.Value);
    }

    [HttpPost("{id:guid}/refund")]
    [HasPermission("payments.refund")]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RefundPayment(
        Guid tenantId,
        Guid id,
        [FromBody] RefundPaymentRequest request,
        CancellationToken ct)
    {
        var result = await _paymentService.RefundPaymentAsync(tenantId, id, request, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        var dto = result.Value!;
        return CreatedAtAction(nameof(GetById), new { tenantId, id = dto.Id }, dto);
    }

    [HttpDelete("{id:guid}")]
    [HasPermission("payments.delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Guid tenantId,
        Guid id,
        CancellationToken ct)
    {
        var result = await _paymentService.DeleteAsync(tenantId, id, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        return NoContent();
    }

    #region BFF Enrichment

    private async Task<PagedList<PaymentListDto>> EnrichList(
        Guid tenantId, PagedList<PaymentListDto> pagedList, CancellationToken ct)
    {
        var clientIds = pagedList.Items.Select(p => p.ClientId).Distinct().ToList();
        var invoiceIds = pagedList.Items.Select(p => p.InvoiceId).Distinct().ToList();

        var clientMap = new Dictionary<Guid, InvoiceClientRef>();
        if (clientIds.Count > 0)
        {
            var clients = await _clientService.ListAsync(tenantId, new QueryParameters { PageSize = clientIds.Count }, ct);
            foreach (var c in clients.Items.Where(c => clientIds.Contains(c.Id)))
                clientMap[c.Id] = new InvoiceClientRef { Id = c.Id, NameEn = c.NameEn, NameAr = c.NameAr };
        }

        var invoiceMap = new Dictionary<Guid, InvoiceRef>();
        if (invoiceIds.Count > 0)
        {
            var invoices = await _invoiceService.ListAsync(tenantId, new QueryParameters { PageSize = invoiceIds.Count }, ct);
            foreach (var inv in invoices.Items.Where(i => invoiceIds.Contains(i.Id)))
                invoiceMap[inv.Id] = new InvoiceRef { Id = inv.Id, InvoiceNumber = inv.InvoiceNumber };
        }

        var enriched = pagedList.Items.Select(p => p with
        {
            Client = clientMap.GetValueOrDefault(p.ClientId),
            Invoice = invoiceMap.GetValueOrDefault(p.InvoiceId),
        }).ToList();

        return new PagedList<PaymentListDto>(enriched, pagedList.TotalCount, pagedList.Page, pagedList.PageSize);
    }

    private async Task<PaymentDto> EnrichSingle(Guid tenantId, PaymentDto dto, CancellationToken ct)
    {
        InvoiceClientRef? clientRef = null;
        var clientResult = await _clientService.GetByIdAsync(tenantId, dto.ClientId, ct);
        if (clientResult.IsSuccess)
            clientRef = new InvoiceClientRef { Id = clientResult.Value!.Id, NameEn = clientResult.Value.NameEn, NameAr = clientResult.Value.NameAr };

        InvoiceRef? invoiceRef = null;
        var invoiceResult = await _invoiceService.GetByIdAsync(tenantId, dto.InvoiceId, ct: ct);
        if (invoiceResult.IsSuccess)
            invoiceRef = new InvoiceRef { Id = invoiceResult.Value!.Id, InvoiceNumber = invoiceResult.Value.InvoiceNumber };

        return dto with { Client = clientRef, Invoice = invoiceRef };
    }

    #endregion

    #region Error Helpers

    private IActionResult MapResultError<T>(Result<T> result)
        => MapError(result.Error!, result.ErrorCode);

    private IActionResult MapResultError(Result result)
        => MapError(result.Error!, result.ErrorCode);

    private IActionResult MapError(string error, string? errorCode)
    {
        var path = HttpContext.Request.Path.Value;
        var (status, apiError) = errorCode switch
        {
            "NOT_FOUND" => (404, ApiError.NotFound(error, path)),
            "CONFLICT" => (409, ApiError.Conflict(error, path)),
            "FORBIDDEN" => (403, ApiError.Forbidden(error)),
            _ => (400, ApiError.BadRequest(error, path))
        };
        return new ObjectResult(apiError) { StatusCode = status, ContentTypes = { "application/problem+json" } };
    }

    #endregion
}
